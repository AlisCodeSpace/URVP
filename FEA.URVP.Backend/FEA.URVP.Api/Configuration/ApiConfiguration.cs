using System.Text.Json.Serialization;
using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Api.Filters;
using FEA.URVP.Api.Services;

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
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // ValidationFilter owns ModelState invalid responses.
                options.SuppressModelStateInvalidFilter = true;
            });

        services.AddHttpContextAccessor();
        services.AddScoped<ReturnUrlValidationService>();

        return services;
    }
}
