using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using RICHConnect.Backend.Application.Services.Security;
using System.Net.Http;

namespace RICHConnect.Backend.Api.Configuration.Auth
{
    /// <summary>
    /// Configuration for authentication services
    /// Cookie-based authentication is the primary method for Azure AD and Azure B2C
    /// </summary>
    public static class AuthenticationConfiguration
    {
        public const string CookieScheme = "RichConnectCookie";
        public const string AzureAdOidcScheme = "AzureAdOidc";
        public const string AzureB2COidcScheme = "AzureB2COidc";

        /// <summary>
        /// Configure authentication: Cookie + OIDC (primary)
        /// </summary>
        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            // Register FMIS services
            services.AddScoped<RICHConnect.Backend.Application.Services.FMIS.IFmisMembershipChecker, 
                RICHConnect.Backend.Infrastructure.Services.FMIS.FmisMembershipChecker>();

            var authBuilder = services
                .AddAuthentication(options =>
                {
                    // Default scheme uses cookie authentication
                    options.DefaultScheme = CookieScheme;
                    options.DefaultAuthenticateScheme = CookieScheme;
                    options.DefaultChallengeScheme = AzureAdOidcScheme;
                    options.DefaultSignInScheme = CookieScheme;
                })
                // Cookie authentication for browser sessions
                .AddCookie(CookieScheme, options =>
                {
                    var cookieConfig = configuration.GetSection("Auth:Cookie");
                    var expireHours = cookieConfig.GetValue<int?>("ExpireHours") ?? 8;
                    var slidingExpiration = cookieConfig.GetValue<bool?>("SlidingExpiration") ?? true;

                    options.Cookie.Name = "RichConnect.Auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;

                    // CRITICAL: Cross-origin cookie handling for dev vs production
                    // In development with cross-origin frontend (e.g., localhost:3000), we need SameSite=None + Secure
                    // In production with same-origin (SPA served from API), we can use Strict
                    var corsOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                    var hasCrossOriginCors = corsOrigins.Length > 0;
                    
                    if (environment.IsDevelopment() && hasCrossOriginCors)
                    {
                        // Dev with cross-origin frontend: SameSite=None requires Secure
                        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.Cookie.SameSite = SameSiteMode.None;
                    }
                    else if (environment.IsDevelopment())
                    {
                        // Dev with same-origin (BFF proxy): can use Lax + SameAsRequest
                        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                        options.Cookie.SameSite = SameSiteMode.Lax;
                    }
                    else
                    {
                        // Production/Staging: Always secure + Strict (SPA is same-origin)
                        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                        options.Cookie.SameSite = SameSiteMode.Strict;
                    }

                    // Allow manual override via configuration for special cases
                    var configuredSameSite = cookieConfig["SameSite"];
                    if (!string.IsNullOrWhiteSpace(configuredSameSite))
                    {
                        var parsed = ParseSameSiteMode(configuredSameSite);
                        if (parsed.HasValue)
                        {
                            options.Cookie.SameSite = parsed.Value;
                        }
                    }

                    options.Cookie.Path = "/";
                    options.SlidingExpiration = slidingExpiration;
                    options.ExpireTimeSpan = TimeSpan.FromHours(expireHours);
                    
                    // Don't redirect API or background fetch calls to login page
                    options.Events.OnRedirectToLogin = context =>
                    {
                        // SECURITY: Log authentication failures for security monitoring
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.AuthenticationConfiguration");
                        
                        var userId = context.HttpContext.User?.Identity?.IsAuthenticated == true
                            ? context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown"
                            : "anonymous";
                        
                        logger.LogWarning(
                            "Authentication required (401) - Path: {Path}, Method: {Method}, User: {UserId}, TraceId: {TraceId}",
                            context.Request.Path,
                            context.Request.Method,
                            userId,
                            context.HttpContext.TraceIdentifier);
                        
                        if (context.Request.Path.StartsWithSegments("/api") || IsBackgroundFetch(context.HttpContext.Request))
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
                        // SECURITY: Log authorization failures for security monitoring
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.AuthenticationConfiguration");
                        
                        var userId = context.HttpContext.User?.Identity?.IsAuthenticated == true
                            ? context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown"
                            : "anonymous";
                        
                        var userRoles = context.HttpContext.User?.Identity?.IsAuthenticated == true
                            ? string.Join(", ", context.HttpContext.User.Claims
                                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                                .Select(c => c.Value))
                            : "none";
                        
                        logger.LogWarning(
                            "Access denied (403) - Path: {Path}, Method: {Method}, User: {UserId}, Roles: [{Roles}], TraceId: {TraceId}",
                            context.Request.Path,
                            context.Request.Method,
                            userId,
                            userRoles,
                            context.HttpContext.TraceIdentifier);
                        
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
                // Azure AD OpenID Connect for interactive sign-in
                .AddOpenIdConnect(AzureAdOidcScheme, options =>
                {
                    // Get configuration values explicitly (don't use Bind to avoid conflicts)
                    var instance = configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";
                    var tenantId = configuration["AzureAd:TenantId"];
                    var clientId = configuration["AzureAd:ClientId"];
                    var adCallbackPath = configuration["AzureAd:CallbackPath"] ?? "/signin-oidc-ad";
                    
                    // Set Authority explicitly (required for OIDC discovery)
                    if (!string.IsNullOrEmpty(tenantId))
                    {
                        var authority = $"{instance.TrimEnd('/')}/{tenantId}/v2.0";
                        options.Authority = authority;
                        
                        // Explicitly set metadata address to bypass discovery issues
                        options.MetadataAddress = $"{authority}/.well-known/openid-configuration";
                    }
                    else
                    {
                        throw new InvalidOperationException("AzureAd:TenantId is required for Azure AD authentication");
                    }
                    
                    // Set ClientId (required)
                    if (!string.IsNullOrEmpty(clientId))
                    {
                        options.ClientId = clientId;
                    }
                    else
                    {
                        throw new InvalidOperationException("AzureAd:ClientId is required for Azure AD authentication");
                    }
                    
                    // Set CallbackPath
                    options.CallbackPath = adCallbackPath;
                    
                    options.SignInScheme = CookieScheme;
                    
                    // Allow HTTP metadata in development (for local testing)
                    options.RequireHttpsMetadata = !environment.IsDevelopment();
                    
                    // Configure backchannel HTTP handler to handle network/proxy/SSL issues
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        UseProxy = true,
                        ServerCertificateCustomValidationCallback = environment.IsDevelopment() 
                            ? (message, cert, chain, errors) => true  // Bypass SSL in development
                            : null  // Use default validation in production
                    };
                    
                    // Configure metadata refresh interval and timeout
                    options.RefreshInterval = TimeSpan.FromHours(24); // Refresh metadata daily
                    options.AutomaticRefreshInterval = TimeSpan.FromDays(1);
                    options.MaxAge = TimeSpan.FromDays(1);
                    
                    // Use id_token-only flow with form_post (no client secret required)
                    options.ResponseType = "id_token";
                    options.ResponseMode = "form_post";

                    // IMPORTANT:
                    // With response_mode=form_post, the IdP posts back cross-site to our CallbackPath.
                    // Correlation/nonce cookies must be sent on that POST, so they must be SameSite=None + Secure.
                    options.CorrelationCookie.SameSite = SameSiteMode.None;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.NonceCookie.SameSite = SameSiteMode.None;
                    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                    
                    // Force account selection prompt (for testing - can be removed from config later)
                    var forceAccountSelection = configuration.GetValue<bool>("AzureAd:ForceAccountSelection", false);
                    if (forceAccountSelection)
                    {
                        options.Prompt = "select_account";
                    }
                    
                    options.SaveTokens = false; // Don't store tokens in cookie
                    options.GetClaimsFromUserInfoEndpoint = false; // Claims come from id_token
                    options.UseTokenLifetime = false; // Use cookie expiration instead
                    
                    // Ensure required scopes for id_token claims
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    
                    // Map claims
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "preferred_username",
                        RoleClaimType = "role"
                    };
                    
                    // Wire event handlers for user provisioning and claims enrichment (Phase 3)
                    options.Events = OidcEventHandlers.CreateAzureAdOidcEvents(configuration);
                })
                // Azure B2C OpenID Connect for social login
                .AddOpenIdConnect(AzureB2COidcScheme, options =>
                {
                    var b2cAuthority = configuration["AzureB2C:Authority"];
                    var b2cClientId = configuration["AzureB2C:ClientId"];
                    var b2cCallbackPath = configuration["AzureB2C:CallbackPath"] ?? "/signin-oidc";
                    var b2cOpenIdConfigUrl = configuration["AzureB2C:OpenIdConfigUrl"];
                    
                    if (string.IsNullOrWhiteSpace(b2cAuthority))
                    {
                        throw new InvalidOperationException("AzureB2C:Authority is required for Azure B2C authentication");
                    }

                    if (string.IsNullOrWhiteSpace(b2cClientId))
                    {
                        throw new InvalidOperationException("AzureB2C:ClientId is required for Azure B2C authentication");
                    }

                    options.Authority = b2cAuthority.TrimEnd('/');

                    // Explicitly set metadata address for B2C (prefer the known OpenIdConfigUrl if provided)
                    options.MetadataAddress = !string.IsNullOrWhiteSpace(b2cOpenIdConfigUrl)
                        ? b2cOpenIdConfigUrl
                        : $"{options.Authority}/.well-known/openid-configuration";

                    options.ClientId = b2cClientId;
                    options.CallbackPath = b2cCallbackPath;
                    options.SignInScheme = CookieScheme;
                    
                    // Allow HTTP metadata in development (for local testing)
                    options.RequireHttpsMetadata = !environment.IsDevelopment();
                    
                    // Configure backchannel HTTP handler for B2C
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        UseProxy = true,
                        ServerCertificateCustomValidationCallback = environment.IsDevelopment() 
                            ? (message, cert, chain, errors) => true  // Bypass SSL in development
                            : null  // Use default validation in production
                    };
                    
                    // Use id_token-only flow with form_post (no client secret required)
                    options.ResponseType = "id_token";
                    options.ResponseMode = "form_post";

                    // IMPORTANT:
                    // With response_mode=form_post, the IdP posts back cross-site to our CallbackPath.
                    // Correlation/nonce cookies must be sent on that POST, so they must be SameSite=None + Secure.
                    options.CorrelationCookie.SameSite = SameSiteMode.None;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.NonceCookie.SameSite = SameSiteMode.None;
                    options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
                    
                    // Force identity provider selection page (don't auto-redirect to last used provider)
                    options.Prompt = "select_account";
                    
                    options.SaveTokens = false; // Don't store tokens in cookie
                    options.GetClaimsFromUserInfoEndpoint = false; // B2C includes claims in id_token
                    options.UseTokenLifetime = false; // Use cookie expiration instead
                    
                    // Ensure required scopes for id_token claims
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    
                    // Map claims for B2C
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                    
                    // Wire event handlers for user provisioning and claims enrichment
                    options.Events = OidcEventHandlers.CreateAzureB2COidcEvents(configuration);
                });

            return services;
        }

        /// <summary>
        /// Detect programmatic fetch requests that must not receive a 302 redirect to an
        /// external IdP (the cross-origin redirect would be blocked by CORS).
        /// Covers Next.js RSC prefetches and general XHR/fetch calls.
        /// </summary>
        private static bool IsBackgroundFetch(HttpRequest request)
        {
            if (request.Headers.ContainsKey("RSC"))
                return true;

            if (request.Headers.ContainsKey("Next-Router-Prefetch"))
                return true;

            if (string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        private static SameSiteMode? ParseSameSiteMode(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

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
}
