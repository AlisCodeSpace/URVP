using RICHConnect.Backend.Application.Interfaces.Search;
using RICHConnect.Backend.Application.Interfaces.Analytics;
using RICHConnect.Backend.Application.Interfaces.Archiving;
using RICHConnect.Backend.Application.Interfaces.ResearchFields;
using RICHConnect.Backend.Application.Services.Search;
using RICHConnect.Backend.Application.Services.Analytics;
using RICHConnect.Backend.Application.Services.Archiving;
using RICHConnect.Backend.Application.Services.ResearchFields;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for infrastructure and cross-cutting services
    /// </summary>
    public static class InfrastructureServicesConfiguration
    {
        /// <summary>
        /// Register search, analytics, archiving, and catalog services
        /// </summary>
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Search Services
            services.AddScoped<ISearchIndexingService, SearchIndexingService>();
            
            // Analytics Services
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            
            // Archiving Services
            services.AddScoped<IArchivingService, ArchivingService>();
            
            // Research Field Catalog Services
            services.AddScoped<IResearchFieldCatalogService, ResearchFieldCatalogService>();
            
            return services;
        }
    }
}
