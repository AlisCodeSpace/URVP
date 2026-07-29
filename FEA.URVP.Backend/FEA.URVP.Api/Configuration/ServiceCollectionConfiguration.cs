using FEA.URVP.Application;
using FEA.URVP.Infrastructure;

namespace FEA.URVP.Api.Configuration;

/// <summary>
/// Composition root for service registration.
/// </summary>
public static class ServiceCollectionConfiguration
{
    public static IServiceCollection ConfigureAllServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddApiServices()
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddOpenApi();

        return services;
    }
}
