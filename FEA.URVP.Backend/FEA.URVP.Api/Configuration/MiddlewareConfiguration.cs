using FEA.URVP.Api.Configuration.Frontend;
using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Middleware;
using Serilog;
using Serilog.Events;

namespace FEA.URVP.Api.Configuration;

/// <summary>
/// HTTP request pipeline. Ordering here is load-bearing; see the comments on each stage.
/// </summary>
public static class MiddlewareConfiguration
{
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        // 1. Forwarded headers first. Everything downstream that reads Request.Scheme, Host or
        //    the client IP (cookie Secure decisions, HTTPS redirection, OIDC redirect URIs, rate
        //    limit partitioning) must see the real public values.
        app.UseForwardedHeaders();

        // 2. Security headers before the exception handler, so its OnStarting callback is already
        //    registered and even an error response carries the full header set.
        app.UseSecurityHeaders();

        // 3. Global exception handling, early enough to own every failure below it.
        app.UseGlobalExceptionHandling();

        // Serilog owns request logging (a single event per request); do not pair this with a
        // custom request-logging middleware, which would duplicate every event.
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (_, elapsedMs, exception) => exception is not null
                ? LogEventLevel.Error
                : elapsedMs > 1000
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;
        });

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();

            if (TransportSecurityConfiguration.ShouldRedirectToHttps(app.Configuration))
            {
                app.UseHttpsRedirection();
            }
        }

        // 4. Static assets of the exported frontend. Deliberately ahead of the rate limiter and
        //    authentication: bundles and images are public, and a page load pulling dozens of
        //    them must not consume the caller's API budget.
        app.UseExportedFrontendAssets();

        app.UseRouting();

        // 5. After UseRouting so per-endpoint [EnableRateLimiting] policies resolve.
        app.UseRateLimiter();

        app.UseCookiePolicy();
        app.UseCors(CorsConfiguration.PolicyName);
        app.UseAuthentication();
        app.UseAuthorization();

        // 6. After authentication so a rejected request can be logged with its caller identity.
        app.UseAntiforgeryValidation();

        app.MapControllers();
        app.MapApiSchema();

        // 7. Terminal: browser navigations resolve to a prerendered HTML document with a fresh
        //    CSP nonce. Unmatched /api paths still get the JSON envelope.
        app.MapExportedFrontend();

        app.ValidateSecurityConfiguration();

        return app;
    }
}
