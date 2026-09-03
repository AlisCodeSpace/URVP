using System.Security.Claims;

namespace FEA.URVP.Api.Configuration.Auth;

/// <summary>
/// Structured logging for authentication and authorization failures.
/// </summary>
/// <remarks>
/// Records only request shape and caller identity. Cookies, tokens, Authorization headers, OIDC
/// state/nonce values and any other credential material are deliberately never read here, so the
/// log sink (Seq) can be treated as lower-trust than the application itself.
/// </remarks>
public static class AuthFailureLogger
{
    private const string LoggerCategory = "FEA.URVP.Api.Security.Authentication";

    public static void LogUnauthenticated(HttpContext context, string reason)
    {
        Logger(context).LogWarning(
            "Unauthenticated request rejected. Reason: {Reason}. {Method} {Path}. TraceId: {TraceId}",
            reason,
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);
    }

    public static void LogForbidden(HttpContext context)
    {
        Logger(context).LogWarning(
            "Authorization denied for {Method} {Path}. User: {UserId}. Roles: {Roles}. TraceId: {TraceId}",
            context.Request.Method,
            context.Request.Path,
            UserId(context),
            Roles(context),
            context.TraceIdentifier);
    }

    public static void LogAuthenticationFailed(HttpContext context, string reason)
    {
        Logger(context).LogError(
            "Authentication failed during sign-in. Reason: {Reason}. {Method} {Path}. TraceId: {TraceId}",
            reason,
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);
    }

    private static string UserId(HttpContext context) =>
        context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

    private static string Roles(HttpContext context)
    {
        var roles = context.User?.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray() ?? [];
        return roles.Length == 0 ? "none" : string.Join(',', roles);
    }

    private static ILogger Logger(HttpContext context) =>
        context.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(LoggerCategory);
}
