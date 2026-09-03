using System.Security.Claims;
using System.Text.Json;
using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Contracts;
using Microsoft.AspNetCore.Antiforgery;

namespace FEA.URVP.Api.Middleware;

/// <summary>
/// Requires a valid antiforgery token on every mutating API request.
/// </summary>
/// <remarks>
/// Applied as middleware rather than an MVC filter so a newly added controller action cannot
/// silently opt out of CSRF protection by forgetting an attribute.
/// <para>
/// Two paths are deliberately outside the check. The Azure AD callback (<c>/signin-oidc-ad</c>)
/// is a cross-site <c>form_post</c> by protocol design and is protected instead by the OIDC
/// state, correlation and nonce validation. The CSP report endpoint is posted directly by the
/// browser, which cannot attach a custom header; it is anonymous, size-capped and rate-limited.
/// </para>
/// </remarks>
public sealed class AntiforgeryValidationMiddleware
{
    /// <summary>Stable code the frontend matches on to refresh its token and retry once.</summary>
    public const string FailureCode = "antiforgery_validation_failed";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly IAntiforgery _antiforgery;
    private readonly ILogger<AntiforgeryValidationMiddleware> _logger;

    public AntiforgeryValidationMiddleware(
        RequestDelegate next,
        IAntiforgery antiforgery,
        ILogger<AntiforgeryValidationMiddleware> logger)
    {
        _next = next;
        _antiforgery = antiforgery;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!RequiresValidation(context.Request))
        {
            await _next(context);
            return;
        }

        try
        {
            await _antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            // The exception message can name cookie and field values, so it is never logged or
            // returned; only the request shape and caller identity are recorded.
            _logger.LogWarning(
                "Antiforgery validation failed for {Method} {Path}. User: {UserId}. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous",
                context.TraceIdentifier);

            await WriteRejectionAsync(context);
            return;
        }

        await _next(context);
    }

    private static bool RequiresValidation(HttpRequest request)
    {
        if (!request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (request.Path.StartsWithSegments("/api/security/csp-report", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return HttpMethods.IsPost(request.Method)
            || HttpMethods.IsPut(request.Method)
            || HttpMethods.IsPatch(request.Method)
            || HttpMethods.IsDelete(request.Method);
    }

    private static async Task WriteRejectionAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var body = ApiResponse<object>.ErrorResult(
            "Invalid or missing request verification token.",
            [FailureCode]);
        body.TraceId = context.TraceIdentifier;

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";
        context.Response.Headers["X-Antiforgery-Required"] = AntiforgeryConfiguration.HeaderName;

        await context.Response.WriteAsync(JsonSerializer.Serialize(body, SerializerOptions));
    }
}
