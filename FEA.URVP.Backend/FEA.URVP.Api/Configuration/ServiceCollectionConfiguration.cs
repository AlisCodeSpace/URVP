using FEA.URVP.Api.Configuration.Auth;
using FEA.URVP.Api.Configuration.Security;
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
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddForwardedHeadersSupport()
            .AddApiServices()
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddCorsPolicy(configuration, environment)
            .AddCookiePolicyConfiguration(environment)
            .AddUrvpAuthentication(configuration, environment)
            .AddAuthorizationPolicies()
            .AddOpenApi();

        return services;
    }
}
