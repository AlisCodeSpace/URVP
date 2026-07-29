using FluentValidation;
using FluentValidation.AspNetCore;
using RICHConnect.Backend.Application.Behaviors;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for FluentValidation services with assembly scanning
    /// </summary>
    public static class ValidationConfiguration
    {
        /// <summary>
        /// Configure FluentValidation services with assembly scanning
        /// </summary>
        public static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            // Configure FluentValidation auto-validation
            services.AddFluentValidationAutoValidation();
            
            // Scan for validators in both API and Application assemblies
            services.AddValidatorsFromAssemblyContaining<Program>(); // API assembly
            services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly); // Application assembly
            
            return services;
        }
    }
}
