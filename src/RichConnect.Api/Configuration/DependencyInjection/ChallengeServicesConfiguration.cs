using RICHConnect.Backend.Application.Interfaces;
using RICHConnect.Backend.Application.Services.Challenges;
using RICHConnect.Backend.Application.Validators.Challenges;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using RICHConnect.Backend.Application.Interfaces.Challenges;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for challenge-related services
    /// </summary>
    public static class ChallengeServicesConfiguration
    {
        /// <summary>
        /// Register all challenge-related services
        /// </summary>
        public static IServiceCollection AddChallengeServices(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IChallengeRepository, ChallengeRepository>();
            services.AddScoped<IChallengeEditRequestRepository, ChallengeEditRequestRepository>();
            
            // Application Services
            services.AddScoped<IChallengeApplicationService, ChallengeApplicationService>();
            services.AddScoped<IChallengeMatchingService, ChallengeMatchingService>();
            
            // Business Rules
            services.AddScoped<ChallengeBusinessRulesService>();
            services.AddScoped<ChallengeBusinessRulesValidator>();
            
            return services;
        }
    }
}
