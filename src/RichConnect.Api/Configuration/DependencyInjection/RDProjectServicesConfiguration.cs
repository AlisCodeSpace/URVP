using RICHConnect.Backend.Application.Interfaces.RDProjects;
using RICHConnect.Backend.Application.Services.RDProjects;
using RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects;
using RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects.Interfaces;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    public static class RDProjectServicesConfiguration
    {
        public static IServiceCollection AddRDProjectServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IRDProjectRepository, RDProjectRepository>();
            
            // Application Services
            services.AddScoped<IRDProjectApplicationService, RDProjectApplicationService>();
            
            return services;
        }
    }
}
