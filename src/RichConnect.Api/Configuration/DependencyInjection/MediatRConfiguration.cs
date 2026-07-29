using MediatR;
using RICHConnect.Backend.Application.Behaviors;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for MediatR services with assembly scanning
    /// </summary>
    public static class MediatRConfiguration
    {
        /// <summary>
        /// Configure MediatR services with assembly scanning and pipeline behaviors
        /// </summary>
        public static IServiceCollection AddMediatRServices(this IServiceCollection services)
        {
            // Register MediatR and scan for handlers from the assembly containing ValidationBehavior (MediatR 12 uses configuration callback)
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ValidationBehavior<,>).Assembly));
            
            // Register pipeline behaviors
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            
            return services;
        }
    }
}
