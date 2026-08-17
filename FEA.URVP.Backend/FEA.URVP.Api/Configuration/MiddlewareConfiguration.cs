using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Middleware;

namespace FEA.URVP.Api.Configuration;

/// <summary>
/// HTTP request pipeline configuration.
/// </summary>
public static class MiddlewareConfiguration
{
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseGlobalExceptionHandling();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseRequestLogging();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            // Render (and similar PaaS) terminate TLS and set PORT. Redirecting
            // HTTP→HTTPS inside the container causes loops without a public HTTPS port.
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PORT")))
            {
                app.UseHttpsRedirection();
            }
        }

        app.UseRouting();
        app.UseCookiePolicy();
        app.UseCors(CorsConfiguration.PolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
