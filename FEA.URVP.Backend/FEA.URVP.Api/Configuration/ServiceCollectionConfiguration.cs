using FEA.URVP.Api.Configuration.Auth;
using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Services;
using FEA.URVP.Application;
using FEA.URVP.Application.Options;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Infrastructure;
using Microsoft.AspNetCore.Http.Features;

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
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));

        // Singleton: it caches exported HTML keyed by file timestamp for the process lifetime.
        services.AddSingleton<ExportedFrontendProvider>();

        services
            .AddForwardedHeadersSupport(configuration)
            .AddApiServices()
            .AddApplication(configuration)
            .AddInfrastructure(configuration)
            .AddCorsPolicy(configuration, environment)
            .AddCookiePolicyConfiguration(environment)
            .AddUrvpAuthentication(configuration, environment)
            .AddAuthorizationPolicies()
            .AddUrvpAntiforgery(configuration, environment)
            .AddUrvpRateLimiting(configuration)
            .AddHstsPolicy(configuration)
            .AddApiSchema(environment);

        ConfigureUploadLimits(services, configuration);

        return services;
    }

    /// <summary>
    /// Caps multipart bodies at the same figure Kestrel enforces, so an oversized upload is
    /// rejected before any of it is buffered.
    /// </summary>
    private static void ConfigureUploadLimits(IServiceCollection services, IConfiguration configuration)
    {
        var maxTotalSizeBytes = configuration.GetSection(FileStorageOptions.SectionName)
            .Get<FileStorageOptions>()?.MaxTotalSizeBytes ?? FileStorageCatalog.MaxTotalSizeBytes;

        if (maxTotalSizeBytes <= 0)
        {
            maxTotalSizeBytes = FileStorageCatalog.MaxTotalSizeBytes;
        }

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = maxTotalSizeBytes;
            options.ValueLengthLimit = 1024 * 1024;
            options.MultipartHeadersLengthLimit = 16 * 1024;
        });
    }
}
