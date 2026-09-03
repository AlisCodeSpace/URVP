using FEA.URVP.Api.Configuration.Security;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace FEA.URVP.Api.Middleware;

/// <summary>
/// Writes the authoritative security headers for every response.
/// </summary>
/// <remarks>
/// The exported Next.js app cannot set its own headers (no Next server runs in production and
/// <c>next.config.ts</c> headers are inert for static files), so this middleware is the single
/// source of truth. Headers are applied from an <see cref="HttpResponse.OnStarting"/> callback
/// because the correct policy depends on the final content type, which downstream components
/// only decide once they begin writing.
/// </remarks>
public sealed class SecurityHeadersMiddleware
{
    private static readonly string[] FingerprintHeaders =
    [
        "Server",
        "X-Powered-By",
        "X-AspNet-Version",
        "X-AspNetMvc-Version",
        "X-SourceFiles"
    ];

    private const string PermissionsPolicy =
        "accelerometer=(), autoplay=(), browsing-topics=(), camera=(), display-capture=(), " +
        "encrypted-media=(), fullscreen=(self), geolocation=(), gyroscope=(), magnetometer=(), " +
        "microphone=(), midi=(), payment=(), picture-in-picture=(), publickey-credentials-get=(), " +
        "screen-wake-lock=(), serial=(), usb=(), xr-spatial-tracking=()";

    private readonly RequestDelegate _next;
    private readonly SecurityOptions _options;

    public SecurityHeadersMiddleware(RequestDelegate next, IOptions<SecurityOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(static state =>
        {
            var (httpContext, options) = ((HttpContext, SecurityOptions))state;
            Apply(httpContext, options);
            return Task.CompletedTask;
        }, (context, _options));

        return _next(context);
    }

    private static void Apply(HttpContext context, SecurityOptions options)
    {
        var headers = context.Response.Headers;

        foreach (var header in FingerprintHeaders)
        {
            headers.Remove(header);
        }

        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = PermissionsPolicy;
        headers["Cross-Origin-Opener-Policy"] = "same-origin";

        var isDocument = IsHtml(context.Response.ContentType);
        var cspOptions = options.ContentSecurityPolicy;

        // frame-ancestors is the modern control; X-Frame-Options is kept for older browsers and
        // is only meaningful on documents.
        if (isDocument && cspOptions.FrameAncestors.Count == 0)
        {
            headers[HeaderNames.XFrameOptions] = "DENY";
        }

        headers[ContentSecurityPolicy.HeaderName(cspOptions)] = isDocument
            ? ContentSecurityPolicy.BuildForDocument(cspOptions, CspNonce.Get(context))
            : ContentSecurityPolicy.BuildForApi(cspOptions);

        ApplyCachePolicy(context, isDocument);
    }

    /// <summary>
    /// Marks sensitive dynamic responses uncacheable. Endpoints that deliberately opt into
    /// caching (hashed static assets, public workshop posters) already set Cache-Control and
    /// are left untouched.
    /// </summary>
    private static void ApplyCachePolicy(HttpContext context, bool isDocument)
    {
        var headers = context.Response.Headers;

        if (!StringValues.IsNullOrEmpty(headers.CacheControl))
        {
            return;
        }

        var isApi = context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);

        if (isApi || isDocument)
        {
            headers.CacheControl = "no-store, no-cache, must-revalidate";
            headers.Pragma = "no-cache";
        }
    }

    private static bool IsHtml(string? contentType) =>
        contentType is not null
        && contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase);
}
