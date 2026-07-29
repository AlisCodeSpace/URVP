namespace RICHConnect.Backend.Api.Configuration.Auth
{
    /// <summary>
    /// Configuration for authorization policies
    /// </summary>
    public static class AuthorizationConfiguration
    {
        /// <summary>
        /// Configure authorization policies
        /// </summary>
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                // SECURITY: Require authentication by default for all endpoints
                // Endpoints must explicitly use [AllowAnonymous] to be public
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            return services;
        }
    }
}
