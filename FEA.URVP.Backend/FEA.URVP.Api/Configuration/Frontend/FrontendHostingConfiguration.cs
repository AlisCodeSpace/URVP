using System.Text.Json;
using FEA.URVP.Api.Contracts;
using FEA.URVP.Api.Middleware;
using FEA.URVP.Api.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace FEA.URVP.Api.Configuration.Frontend;

/// <summary>
/// Serves the statically exported Next.js app from this process, making the browser origin and
/// the API origin identical (same-origin BFF).
/// </summary>
public static class FrontendHostingConfiguration
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serves hashed bundles and public assets. Runs before routing and authentication: the
    /// export is entirely public, and authorization is enforced only by the API it calls.
    /// </summary>
    public static WebApplication UseExportedFrontendAssets(this WebApplication app)
    {
        var frontend = app.Services.GetRequiredService<ExportedFrontendProvider>();
        if (!frontend.IsEnabled)
        {
            return app;
        }

        // HTML is deliberately unmapped so documents fall through to the fallback endpoint,
        // which is the only place a CSP nonce can be injected.
        var contentTypes = new FileExtensionContentTypeProvider();
        contentTypes.Mappings.Remove(".html");
        contentTypes.Mappings.Remove(".htm");

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(frontend.RootPath!),
            ContentTypeProvider = contentTypes,
            ServeUnknownFileTypes = false,
            RedirectToAppendTrailingSlash = false,
            OnPrepareResponse = context =>
            {
                var isImmutable = context.Context.Request.Path
                    .StartsWithSegments("/_next/static", StringComparison.OrdinalIgnoreCase);

                context.Context.Response.Headers.CacheControl = isImmutable
                    ? "public, max-age=31536000, immutable"
                    : "public, max-age=3600";
            }
        });

        return app;
    }

    /// <summary>
    /// Terminal handler for browser navigations. Unmatched API paths keep returning the JSON
    /// envelope rather than an HTML document.
    /// </summary>
    /// <remarks>
    /// The route pattern is <c>{*path}</c> rather than the <c>MapFallback</c> default of
    /// <c>{*path:nonfile}</c> on purpose. With the <c>nonfile</c> constraint, any request whose
    /// last segment contains a dot matches no endpoint at all, and the authorization middleware
    /// then applies the fallback policy to the null endpoint — so a missing asset answered 401
    /// and wrote an authentication-failure warning. That let an anonymous caller flood the log
    /// with fake auth failures simply by requesting <c>/a.json</c>, <c>/b.json</c> and so on,
    /// burying real ones. Matching every path keeps a missing file an anonymous 404.
    /// </remarks>
    public static WebApplication MapExportedFrontend(this WebApplication app)
    {
        app.MapFallback("{*path}", async (HttpContext context, ExportedFrontendProvider frontend) =>
        {
            var path = context.Request.Path;

            if (path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                || !frontend.IsEnabled
                || IsAssetRequest(path))
            {
                await WriteJsonNotFoundAsync(context);
                return;
            }

            if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return;
            }

            var document = frontend.GetDocument(path.Value ?? "/", CspNonce.Get(context));
            if (document is null)
            {
                await WriteJsonNotFoundAsync(context);
                return;
            }

            context.Response.StatusCode = document.StatusCode;
            context.Response.ContentType = "text/html; charset=utf-8";

            if (HttpMethods.IsHead(context.Request.Method))
            {
                return;
            }

            await context.Response.WriteAsync(document.Html);
        })
        .AllowAnonymous()
        .ExcludeFromDescription();

        return app;
    }

    /// <summary>
    /// A request for a missing asset gets the JSON envelope; only real navigations are worth
    /// spending an HTML document on. The static-file middleware has already served everything
    /// that exists by the time this runs.
    /// </summary>
    private static bool IsAssetRequest(PathString path)
    {
        var value = path.Value;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        var lastSegment = value.AsSpan(value.LastIndexOf('/') + 1);
        return lastSegment.Contains('.');
    }

    private static async Task WriteJsonNotFoundAsync(HttpContext context)
    {
        var response = ApiResponse<object>.ErrorResult("Resource not found");
        response.TraceId = context.TraceIdentifier;

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
    }
}
