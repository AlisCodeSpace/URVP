using RICHConnect.Backend.Application.Interfaces.Partners;
using RICHConnect.Backend.Application.Services.Partners;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for partner-related services
    /// Phase 6: Removed ILogoUploadService (now handled by unified DatabaseFileUploadService)
    /// </summary>
    public static class PartnerServicesConfiguration
    {
        /// <summary>
        /// Register all partner-related services
        /// </summary>
        public static IServiceCollection AddPartnerServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IPartnerRepository, PartnerRepository>();
            
            // Application Services
            services.AddScoped<IPartnerApplicationService, PartnerApplicationService>();
            services.AddScoped<PartnerBusinessRulesService>();
            
            // Note: File upload now handled by DatabaseFileUploadService (registered in ApplicationServicesConfiguration)
            
            return services;
        }
    }
}
