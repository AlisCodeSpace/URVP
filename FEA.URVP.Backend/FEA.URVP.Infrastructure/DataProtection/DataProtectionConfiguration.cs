using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FEA.URVP.Infrastructure.DataProtection;

/// <summary>
/// ASP.NET Core Data Protection key ring: shared across instances, durable across IIS recycle
/// and wipe-and-unzip deploys, and encrypted at rest so a SQL dump cannot unprotect cookies.
/// </summary>
public static class DataProtectionConfiguration
{
    public const string ApplicationName = "FEA.URVP.Backend";

    public const string CertificateThumbprintKey = "DataProtection:CertificateThumbprint";

    public static IServiceCollection AddUrvpDataProtection(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dataProtection = services.AddDataProtection()
            .SetApplicationName(ApplicationName)
            .PersistKeysToDbContext<AppDbContext>();

        var thumbprint = ReadCertificateThumbprint(configuration);
        if (thumbprint is not null)
        {
            dataProtection.ProtectKeysWithCertificate(thumbprint);
        }
        else if (OperatingSystem.IsWindows())
        {
            // User-level DPAPI. On IIS the app pool must have Load User Profile = true so the
            // identity has a real DPAPI store; the deploy script sets that on the pool.
            dataProtection.ProtectKeysWithDpapi();
        }

        return services;
    }

    public static string? ReadCertificateThumbprint(IConfiguration configuration)
    {
        var value = configuration[CertificateThumbprintKey];
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// True when newly generated keys will be encrypted before they are written to SQL.
    /// Existing plaintext rows remain readable so adding protection does not invalidate sessions.
    /// </summary>
    public static bool EncryptsKeysAtRest(IConfiguration configuration) =>
        ReadCertificateThumbprint(configuration) is not null || OperatingSystem.IsWindows();
}
