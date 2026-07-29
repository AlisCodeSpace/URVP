using AspNetCoreRateLimit;

namespace RICHConnect.Backend.Api.Configuration.RateLimiting
{
    /// <summary>
    /// Configuration for rate limiting to protect against DoS attacks
    /// </summary>
    public static class RateLimitConfiguration
    {
        /// <summary>
        /// Configure rate limiting services
        /// </summary>
        public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            // Load general rate limit configuration from appsettings
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
            
            // Inject counter and rules stores
            services.AddMemoryCache();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, AspNetCoreRateLimit.RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

            return services;
        }

        /// <summary>
        /// Use rate limiting middleware
        /// </summary>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
        {
            return app.UseIpRateLimiting();
        }
    }
}
