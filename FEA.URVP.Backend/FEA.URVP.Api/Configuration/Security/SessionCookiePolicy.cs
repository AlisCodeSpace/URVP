namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Single source of truth for how first-party session cookies (authentication, antiforgery) are
/// scoped. Shared so the auth cookie and the antiforgery cookie can never drift apart.
/// </summary>
public static class SessionCookiePolicy
{
    /// <summary>
    /// Resolves SameSite and Secure for a first-party session cookie.
    /// </summary>
    /// <remarks>
    /// Production serves the exported frontend from this process, so every API call is a
    /// same-origin subresource request and <see cref="SameSiteMode.Strict"/> holds. The OIDC
    /// callback is the only cross-site hop; it relies on the correlation and nonce cookies
    /// (SameSite=None) rather than on the session cookie, and the post-login redirect only needs
    /// to deliver a public HTML document.
    /// </remarks>
    public static (SameSiteMode SameSite, CookieSecurePolicy Secure) Resolve(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return (SameSiteMode.Strict, CookieSecurePolicy.Always);
        }

        // Local `next dev` serves the frontend from a different port, which makes every API call
        // cross-site. SameSite=None is the minimum relaxation that topology needs, and it still
        // requires Secure (the dev servers both run HTTPS).
        var isSplitOrigin = CorsOrigins.GetAllowedOrigins(configuration, allowInsecureLoopback: true).Length > 0;

        return isSplitOrigin
            ? (SameSiteMode.None, CookieSecurePolicy.Always)
            : (SameSiteMode.Lax, CookieSecurePolicy.SameAsRequest);
    }

    public static SameSiteMode? ParseSameSiteMode(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "strict" => SameSiteMode.Strict,
            "lax" => SameSiteMode.Lax,
            "none" => SameSiteMode.None,
            "unspecified" => SameSiteMode.Unspecified,
            _ => null
        };
}
