using StackExchange.Redis;

namespace RICHConnect.Backend.Api.Configuration.Caching
{
    /// <summary>
    /// Configuration for distributed caching (Redis) and in-memory caching
    /// </summary>
    public static class CachingConfiguration
    {
        /// <summary>
        /// Configure Redis distributed cache
        /// Falls back to in-memory cache if Redis connection string is not configured
        /// Also registers IMemoryCache for local in-memory caching
        /// </summary>
        public static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration configuration)
        {
            // Register IMemoryCache for local in-memory caching (used by FMIS membership checker)
            services.AddMemoryCache();
            
            var redisConnectionString = configuration.GetConnectionString("Redis");
            
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                // Configure Redis distributed cache
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "RICHConnect:";
                });
                
                // Register IConnectionMultiplexer for advanced Redis operations if needed
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    return ConnectionMultiplexer.Connect(redisConnectionString);
                });
            }
            else
            {
                // Fallback to in-memory cache if Redis is not configured
                services.AddDistributedMemoryCache();
            }
            
            return services;
        }
    }
}

