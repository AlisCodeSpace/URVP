using Microsoft.AspNetCore.CookiePolicy;

namespace RICHConnect.Backend.Api.Configuration.Security
{
    /// <summary>
    /// Centralized cookie policy enforcement (Secure/HttpOnly) to avoid drifting defaults.
    /// IMPORTANT: Do not set a global MinimumSameSitePolicy that would override OIDC
    /// correlation/nonce cookies (which must be SameSite=None).
    /// </summary>
    public static class CookiePolicyConfiguration
    {
        public static IServiceCollection AddCookiePolicyConfiguration(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // Leave SameSite unspecified globally to avoid breaking OIDC (correlation/nonce need None).
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;

                // Enforce HttpOnly on cookies unless explicitly overridden.
                options.HttpOnly = HttpOnlyPolicy.Always;

                // Enforce Secure in non-dev; in dev, honor request scheme for local http.
                options.Secure = environment.IsDevelopment()
                    ? CookieSecurePolicy.SameAsRequest
                    : CookieSecurePolicy.Always;

                // Defense-in-depth: ensure any SameSite=None cookies are also Secure (browser requirement).
                options.OnAppendCookie = ctx =>
                {
                    if (ctx.CookieOptions.SameSite == SameSiteMode.None)
                    {
                        ctx.CookieOptions.Secure = true;
                    }
                };
                options.OnDeleteCookie = ctx =>
                {
                    if (ctx.CookieOptions.SameSite == SameSiteMode.None)
                    {
                        ctx.CookieOptions.Secure = true;
                    }
                };
            });

            return services;
        }
    }
}

