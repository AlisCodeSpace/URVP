namespace FEA.URVP.Api.Configuration.Auth;

/// <summary>
/// Authorization policies. Authenticated by default; opt out with [AllowAnonymous].
/// </summary>
public static class AuthorizationConfiguration
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
