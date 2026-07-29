using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Api.Filters;

namespace FEA.URVP.Api.Configuration;

/// <summary>
/// ASP.NET Core API services (controllers, filters, HTTP context).
/// </summary>
public static class ApiConfiguration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
            {
                options.Filters.Add<ValidationFilter>();
            })
            .AddApplicationPart(typeof(ApiControllerBase).Assembly)
            .ConfigureApiBehaviorOptions(options =>
            {
                // ValidationFilter owns ModelState invalid responses.
                options.SuppressModelStateInvalidFilter = true;
            });

        services.AddHttpContextAccessor();

        return services;
    }
}
