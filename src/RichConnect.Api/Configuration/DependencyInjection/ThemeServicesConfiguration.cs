using RICHConnect.Backend.Application.Interfaces.Themes;
using RICHConnect.Backend.Application.Services.Themes;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for theme-related services
    /// </summary>
    public static class ThemeServicesConfiguration
    {
        /// <summary>
        /// Register all theme-related services
        /// </summary>
        public static IServiceCollection AddThemeServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IThemeRepository, ThemeRepository>();
            
            // Application Services
            services.AddScoped<IThemeApplicationService, ThemeApplicationService>();
            services.AddScoped<IThemeBusinessRulesService, ThemeBusinessRulesService>();
            
            return services;
        }
    }
}
