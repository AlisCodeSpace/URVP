using Microsoft.AspNetCore.CookiePolicy;

namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Cookie policy. Do not set a global MinimumSameSitePolicy that would override
/// OIDC correlation/nonce cookies (which must be SameSite=None).
/// </summary>
public static class CookiePolicyConfiguration
{
    public static IServiceCollection AddCookiePolicyConfiguration(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            options.HttpOnly = HttpOnlyPolicy.Always;
            options.Secure = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

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
