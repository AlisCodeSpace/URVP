using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.Utilities.Files;
using RICHConnect.Backend.Application.Services.Files;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for application services (orchestrates all feature-specific configurations)
    /// Phase 1: Added helper utilities for database file storage
    /// Phase 4: Added file read service for database-backed file streaming
    /// Phase 5: Added verification service for migration completeness
    /// </summary>
    public static class ApplicationServicesConfiguration
    {
        /// <summary>
        /// Configure all application services by calling feature-specific extensions
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register file storage helper utilities
            services.AddScoped<IMimeTypeValidator, MimeTypeValidator>();
            services.AddScoped<IContentHashHelper, ContentHashHelper>();

            // Register file read service
            services.AddScoped<IFileReadService, DatabaseFileReadService>();

            // Register shared services
            services.AddScoped<FileStorageFactory>();
            services.AddScoped<IFileUploadService>(provider =>
            {
                var factory = provider.GetRequiredService<FileStorageFactory>();
                return factory.CreateFileStorageService();
            });
            
            // User services
            services.AddScoped<IUserRepository, UserRepository>();
            
            // Register feature-specific services
            return services
                .AddChallengeServices()
                .AddPartnerServices()
                .AddThemeServices()
                .AddResearchFieldServices()
                .AddRDProjectServices()
                .AddNotificationServices()
                .AddSettingsServices()
                .AddInfrastructureServices();
        }
    }
}
