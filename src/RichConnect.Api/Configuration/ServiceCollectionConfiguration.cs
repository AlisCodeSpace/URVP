using RICHConnect.Backend.Api.Configuration.Auth;
using RICHConnect.Backend.Api.Configuration.Caching;
using RICHConnect.Backend.Api.Configuration.Database;
using RICHConnect.Backend.Api.Configuration.Security;
using Hangfire;
using Hangfire.SqlServer;
using RICHConnect.Backend.Api.Configuration.Swagger;
using RICHConnect.Backend.Api.Configuration.DependencyInjection;
using RICHConnect.Backend.Api.Configuration.RateLimiting;
using RICHConnect.Backend.Api.Configuration.HealthChecks;
using RICHConnect.Backend.Application.Services.Security;

namespace RICHConnect.Backend.Api.Configuration
{
    /// <summary>
    /// Main orchestrator for all service configuration
    /// This is the single entry point for configuring all services in the application
    /// </summary>
    public static class ServiceCollectionConfiguration
    {
        /// <summary>
        /// Configure all services in the correct order
        /// </summary>
        public static IServiceCollection ConfigureAllServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            // Core application, infrastructure, and platform services
            services
                .AddDistributedCache(configuration)
                .AddApiServices()
                .AddValidationServices()
                .AddMediatRServices()
                .AddApplicationServices()
                .AddEventInfrastructure()
                .AddDatabaseServices(configuration)
                .AddForwardedHeadersConfiguration(configuration, environment)
                .AddCorsPolicy(configuration, environment)
                .AddCookiePolicyConfiguration(configuration, environment)
                .AddAuthentication(configuration, environment)
                .AddAuthorizationPolicies()
                .AddSwaggerServices()
                .AddRateLimiting(configuration)
                .AddApplicationHealthChecks(configuration)
                .AddSingleton<SecretManager>();

            // Enforce strong HSTS policy in non-development environments.
            // Scanner requires >= 1 year max-age and includeSubDomains.
            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
                options.Preload = true;
            });

            // Hangfire background job processing (uses the same SQL Server as AppDbContext)
            services.AddHangfire((provider, config) =>
            {
                var cfg = provider.GetRequiredService<IConfiguration>();
                var connectionString = cfg.GetConnectionString("SqlServerConnection");

                config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                    {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    });
            });

            services.AddHangfireServer(options =>
            {
                // Dedicated "emails" queue for notification outbox processing
                options.Queues = new[] { "default", "emails" };
            });

            return services;
        }
    }
}