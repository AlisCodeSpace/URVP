using FEA.URVP.Api.Configuration.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace FEA.URVP.Api.Configuration.Auth;

/// <summary>
/// Cookie + Azure AD OIDC authentication for the same-origin BFF.
/// </summary>
/// <remarks>
/// The browser only ever holds the application session cookie. No identity or access token is
/// returned to, or stored by, the frontend: <c>SaveTokens</c> is off and the OIDC response is
/// consumed server-side on the callback.
/// </remarks>
public static class AuthenticationConfiguration
{
    public const string CookieScheme = "UrvpCookie";
    public const string AzureAdOidcScheme = "AzureAdOidc";
    public const string CookieName = "FEA.URVP.Auth";

    private const int DefaultExpireHours = 8;

    public static IServiceCollection AddUrvpAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var authenticationBuilder = services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieScheme;
                options.DefaultAuthenticateScheme = CookieScheme;
                options.DefaultSignInScheme = CookieScheme;

                // Challenging the cookie scheme (not OIDC) is what turns an unauthenticated API
                // call into a 401 instead of a 302 to the identity provider that a fetch() cannot
                // follow. Interactive sign-in still reaches OIDC because AzureAdSsoController
                // names that scheme explicitly.
                options.DefaultChallengeScheme = CookieScheme;
            })
            .AddCookie(CookieScheme, options => ConfigureSessionCookie(options, configuration, environment));

        // The OIDC handler implements IAuthenticationRequestHandler, so its options are built on
        // every request to test for the callback path. Registering it without a tenant and client
        // id would therefore turn one missing setting into a 500 on every route, health probes
        // included. StartupSecurityValidation refuses to boot outside Development when the
        // settings are absent, so skipping the scheme here only ever affects local work.
        if (AzureAdOidcConfiguration.IsConfigured(configuration))
        {
            authenticationBuilder.AddOpenIdConnect(AzureAdOidcScheme, options =>
                AzureAdOidcConfiguration.Configure(options, configuration, environment));
        }

        return services;
    }

    private static void ConfigureSessionCookie(
        CookieAuthenticationOptions options,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var cookieConfig = configuration.GetSection("Auth:Cookie");
        var expireHours = cookieConfig.GetValue<int?>("ExpireHours") ?? DefaultExpireHours;
        if (expireHours <= 0)
        {
            expireHours = DefaultExpireHours;
        }

        options.Cookie.Name = CookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Path = "/";
        options.ExpireTimeSpan = TimeSpan.FromHours(expireHours);
        options.SlidingExpiration = cookieConfig.GetValue<bool?>("SlidingExpiration") ?? true;

        var (sameSite, secure) = SessionCookiePolicy.Resolve(configuration, environment);
        options.Cookie.SameSite = sameSite;
        options.Cookie.SecurePolicy = secure;

        // Escape hatch for a deployment that genuinely splits frontend and API origins. Never
        // weakens Secure: SameSite=None requires it, and everything else keeps the resolved value.
        var configuredSameSite = SessionCookiePolicy.ParseSameSiteMode(cookieConfig["SameSite"]);
        if (configuredSameSite.HasValue)
        {
            options.Cookie.SameSite = configuredSameSite.Value;
            if (configuredSameSite.Value == SameSiteMode.None)
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            }
        }

        options.Events.OnRedirectToLogin = context =>
        {
            AuthFailureLogger.LogUnauthenticated(context.HttpContext, "no valid session cookie");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            AuthFailureLogger.LogForbidden(context.HttpContext);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    }
}
