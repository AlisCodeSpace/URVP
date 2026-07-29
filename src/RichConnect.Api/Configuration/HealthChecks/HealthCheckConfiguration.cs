using Microsoft.Extensions.Diagnostics.HealthChecks;
using RICHConnect.Backend.Api.HealthChecks;

namespace RICHConnect.Backend.Api.Configuration.HealthChecks
{
    /// <summary>
    /// Configuration for ASP.NET Core health checks
    /// Registers health checks for database, Redis cache, and critical external services
    /// </summary>
    public static class HealthCheckConfiguration
    {
        /// <summary>
        /// Configure health checks for critical dependencies
        /// </summary>
        public static IServiceCollection AddApplicationHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var healthChecksBuilder = services.AddHealthChecks();

            // Database health check (critical)
            var sqlConnectionString = configuration.GetConnectionString("SqlServerConnection");
            if (!string.IsNullOrEmpty(sqlConnectionString))
            {
                healthChecksBuilder.AddSqlServer(
                    connectionString: sqlConnectionString,
                    healthQuery: "SELECT 1;",
                    name: "database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "db", "sql", "critical" },
                    timeout: TimeSpan.FromSeconds(5));
            }

            // Redis health check (degraded if unavailable, not critical)
            var redisConnectionString = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                healthChecksBuilder.AddRedis(
                    redisConnectionString: redisConnectionString,
                    name: "redis",
                    failureStatus: HealthStatus.Degraded, // Degraded, not Unhealthy (app can work without Redis)
                    tags: new[] { "cache", "redis" },
                    timeout: TimeSpan.FromSeconds(3));
            }

            // FMIS service health check (degraded if unavailable, not critical)
            var fmisEndpoint = configuration["ServicesConfigurationEndPoint"];
            if (!string.IsNullOrEmpty(fmisEndpoint))
            {
                healthChecksBuilder.AddCheck<FmisHealthCheck>(
                    name: "fmis",
                    failureStatus: HealthStatus.Degraded, // Degraded, not Unhealthy (app can work without FMIS)
                    tags: new[] { "external", "fmis" });
            }

            return services;
        }
    }
}
