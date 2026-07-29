using RICHConnect.Backend.Api.Filters;
using RICHConnect.Backend.Api.Services;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for API services
    /// </summary>
    public static class ApiConfiguration
    {
        /// <summary>
        /// Configure API services including controllers, validation, and behavior options
        /// </summary>
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            services.AddControllers(options =>
            {
                // Register global filters
                options.Filters.Add<ValidationFilter>();
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // We'll handle validation manually with ValidationFilter
            });

            // Register HttpContextAccessor (required for rate limiting and other context-based services)
            services.AddHttpContextAccessor();

            // Register shared services
            services.AddScoped<ReturnUrlValidationService>();

            return services;
        }
    }
}
