using RICHConnect.Backend.Application.Interfaces.ResearchFields;
using RICHConnect.Backend.Application.Services.ResearchFields;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for research field-related services
    /// </summary>
    public static class ResearchFieldServicesConfiguration
    {
        /// <summary>
        /// Register all research field-related services
        /// </summary>
        public static IServiceCollection AddResearchFieldServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IResearchFieldRepository, ResearchFieldRepository>();
            
            // Application Services
            services.AddScoped<IResearchFieldApplicationService, ResearchFieldApplicationService>();
            services.AddScoped<ResearchFieldBusinessRulesService>();
            
            return services;
        }
    }
}
