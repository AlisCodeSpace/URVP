using FEA.URVP.Api.Configuration.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace FEA.URVP.Api.Configuration.Auth;

/// <summary>
/// Cookie + Azure AD OIDC authentication (BFF-style AUB SSO).
/// </summary>
public static class AuthenticationConfiguration
{
    public const string CookieScheme = "UrvpCookie";
    public const string AzureAdOidcScheme = "AzureAdOidc";

    public static IServiceCollection AddUrvpAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieScheme;
                options.DefaultAuthenticateScheme = CookieScheme;
                options.DefaultChallengeScheme = AzureAdOidcScheme;
                options.DefaultSignInScheme = CookieScheme;
            })
            .AddCookie(CookieScheme, options =>
            {
                var cookieConfig = configuration.GetSection("Auth:Cookie");
                var expireHours = cookieConfig.GetValue<int?>("ExpireHours") ?? 8;
                var slidingExpiration = cookieConfig.GetValue<bool?>("SlidingExpiration") ?? true;

                options.Cookie.Name = "FEA.URVP.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Path = "/";
                options.SlidingExpiration = slidingExpiration;
                options.ExpireTimeSpan = TimeSpan.FromHours(expireHours);

                var corsOrigins = CorsOrigins.GetAllowedOrigins(configuration);
                var hasCrossOriginCors = corsOrigins.Length > 0;

                if (hasCrossOriginCors)
                {
                    // SPA and API are different origins (e.g. two Render web services).
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.None;
                }
                else if (environment.IsDevelopment())
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                }
                else
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                }

                var configuredSameSite = cookieConfig["SameSite"];
                if (!string.IsNullOrWhiteSpace(configuredSameSite))
                {
                    var parsed = ParseSameSiteMode(configuredSameSite);
                    if (parsed.HasValue)
                    {
                        options.Cookie.SameSite = parsed.Value;
                        if (parsed.Value == SameSiteMode.None)
                        {
                            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        }
                    }
                }

                options.Events.OnRedirectToLogin = context =>
                {
                    var path = context.Request.Path;
                    if (path.StartsWithSegments("/api")
                        || path == "/"
                        || path.StartsWithSegments("/health")
                        || IsBackgroundFetch(context.HttpContext.Request))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }
                    else
                    {
                        context.Response.Redirect(context.RedirectUri);
                    }

                    return Task.CompletedTask;
                };

                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api") || IsBackgroundFetch(context.HttpContext.Request))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    }
                    else
                    {
                        context.Response.Redirect(context.RedirectUri);
                    }

                    return Task.CompletedTask;
                };
            })
            .AddOpenIdConnect(AzureAdOidcScheme, options =>
            {
                var instance = configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";
                var tenantId = configuration["AzureAd:TenantId"];
                var clientId = configuration["AzureAd:ClientId"];
                var callbackPath = configuration["AzureAd:CallbackPath"] ?? "/signin-oidc-ad";

                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    throw new InvalidOperationException("AzureAd:TenantId is required for Azure AD authentication");
                }

                if (string.IsNullOrWhiteSpace(clientId))
                {
                    throw new InvalidOperationException("AzureAd:ClientId is required for Azure AD authentication");
                }

                var authority = $"{instance.TrimEnd('/')}/{tenantId}/v2.0";
                options.Authority = authority;
                options.MetadataAddress = $"{authority}/.well-known/openid-configuration";
                options.ClientId = clientId;
                options.CallbackPath = callbackPath;
                options.SignInScheme = CookieScheme;
                options.RequireHttpsMetadata = !environment.IsDevelopment();

                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    UseProxy = true,
                    ServerCertificateCustomValidationCallback = environment.IsDevelopment()
                        ? static (_, _, _, _) => true
                        : null
                };

                // id_token-only flow with form_post (no client secret).
                options.ResponseType = "id_token";
                options.ResponseMode = "form_post";

                // form_post is cross-site; correlation/nonce cookies must be SameSite=None + Secure.
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.NonceCookie.SameSite = SameSiteMode.None;
                options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;

                if (configuration.GetValue("AzureAd:ForceAccountSelection", false))
                {
                    options.Prompt = "select_account";
                }

                options.SaveTokens = false;
                options.GetClaimsFromUserInfoEndpoint = false;
                options.UseTokenLifetime = false;

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = "role"
                };

                options.Events = OidcEventHandlers.CreateAzureAdOidcEvents(configuration);
            });

        return services;
    }

    private static bool IsBackgroundFetch(HttpRequest request)
    {
        if (request.Headers.ContainsKey("RSC"))
        {
            return true;
        }

        if (request.Headers.ContainsKey("Next-Router-Prefetch"))
        {
            return true;
        }

        return string.Equals(
            request.Headers["X-Requested-With"],
            "XMLHttpRequest",
            StringComparison.OrdinalIgnoreCase);
    }

    private static SameSiteMode? ParseSameSiteMode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "strict" => SameSiteMode.Strict,
            "lax" => SameSiteMode.Lax,
            "none" => SameSiteMode.None,
            "unspecified" => SameSiteMode.Unspecified,
            _ => null
        };
    }
}
