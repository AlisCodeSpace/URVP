namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Antiforgery for the cookie-authenticated API.
/// </summary>
/// <remarks>
/// SameSite=Strict and a same-origin CORS posture already make cross-site mutation hard, but both
/// are defence in depth rather than a CSRF control, so every mutating API request must also carry
/// an explicit antiforgery token. The frontend reads the token from <c>GET /api/auth/csrf</c> and
/// echoes it in <see cref="HeaderName"/>; the paired cookie stays HttpOnly, so no token value is
/// ever readable from JavaScript or persisted in browser storage.
/// </remarks>
public static class AntiforgeryConfiguration
{
    public const string HeaderName = "X-CSRF-TOKEN";
    public const string CookieName = "FEA.URVP.Antiforgery";

    public static IServiceCollection AddUrvpAntiforgery(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var (sameSite, secure) = SessionCookiePolicy.Resolve(configuration, environment);

        services.AddAntiforgery(options =>
        {
            options.HeaderName = HeaderName;
            options.Cookie.Name = CookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Path = "/";
            options.Cookie.SameSite = sameSite;
            options.Cookie.SecurePolicy = secure;

            // SecurityHeadersMiddleware owns X-Frame-Options for the whole app.
            options.SuppressXFrameOptionsHeader = true;
        });

        return services;
    }
}
