using RICHConnect.Backend.Application.Interfaces.Settings;
using RICHConnect.Backend.Application.Services.Settings;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Settings;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Settings.Interfaces;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for settings (admin-manageable AppSettings) services.
    /// Phase 2: Repository and SettingsService with encryption/masking for secrets.
    /// </summary>
    public static class SettingsServicesConfiguration
    {
        /// <summary>
        /// Register settings repository and application service.
        /// </summary>
        public static IServiceCollection AddSettingsServices(this IServiceCollection services)
        {
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<ISettingsService, SettingsService>();
            return services;
        }
    }
}
