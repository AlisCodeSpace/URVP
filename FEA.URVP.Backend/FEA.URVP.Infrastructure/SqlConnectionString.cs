using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FEA.URVP.Infrastructure;

/// <summary>
/// Normalizes SQL Server connection strings from PaaS dashboards and hardens TLS settings.
/// Hosts like MonsterASP often copy as <c>host;Database=...</c> without <c>Server=</c>,
/// which SqlClient rejects as keyword <c>host;database</c>.
/// Shared SQL (Somee) typically requires <c>TrustServerCertificate=True</c> because the
/// host name does not match the server certificate; that opt-in is preserved when present.
/// </summary>
public static class SqlConnectionString
{
    public const string AllowTrustServerCertificateKey = "SqlServer:AllowTrustServerCertificate";

    public static string Normalize(string connectionString, IConfiguration configuration)
        => Normalize(
            connectionString,
            AllowsTrustServerCertificate(configuration)
            || RequestsTrustServerCertificate(connectionString));

    public static string Normalize(string connectionString, bool allowTrustServerCertificate = false)
    {
        var value = connectionString.Trim().Trim('"').Trim('\'');
        if (value.Length == 0)
        {
            return value;
        }

        if (!HasServerKeyword(value))
        {
            value = $"Server={value}";
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(value)
            {
                MultipleActiveResultSets = true,
                Encrypt = true
            };

            if (!allowTrustServerCertificate)
            {
                builder.TrustServerCertificate = false;
            }

            return builder.ConnectionString;
        }
        catch (ArgumentException)
        {
            return HardenUnparsed(EnsureMultipleActiveResultSets(value), allowTrustServerCertificate);
        }
    }

    public static bool AllowsTrustServerCertificate(IConfiguration configuration)
    {
        if (configuration.GetValue(AllowTrustServerCertificateKey, false))
        {
            return true;
        }

        var environment = configuration["ASPNETCORE_ENVIRONMENT"]
            ?? configuration["DOTNET_ENVIRONMENT"]
            ?? configuration[HostDefaults.EnvironmentKey]
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        return string.Equals(environment, Environments.Development, StringComparison.OrdinalIgnoreCase);
    }

    public static bool RequestsTrustServerCertificate(string connectionString)
    {
        try
        {
            return new SqlConnectionStringBuilder(connectionString).TrustServerCertificate;
        }
        catch (ArgumentException)
        {
            return connectionString.Contains("TrustServerCertificate=true", StringComparison.OrdinalIgnoreCase)
                   || connectionString.Contains("TrustServerCertificate=yes", StringComparison.OrdinalIgnoreCase)
                   || connectionString.Contains("Trust Server Certificate=true", StringComparison.OrdinalIgnoreCase)
                   || connectionString.Contains("Trust Server Certificate=yes", StringComparison.OrdinalIgnoreCase);
        }
    }

    private static bool HasServerKeyword(string value)
    {
        foreach (var key in new[] { "Server", "Data Source", "Addr", "Address", "Network Address" })
        {
            if (value.StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase)
                || value.Contains($";{key}=", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string EnsureMultipleActiveResultSets(string value)
    {
        if (value.Contains("MultipleActiveResultSets", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        return value.TrimEnd(';') + ";MultipleActiveResultSets=true";
    }

    private static string HardenUnparsed(string value, bool allowTrustServerCertificate)
    {
        if (!allowTrustServerCertificate)
        {
            value = StripKeyword(value, "TrustServerCertificate");
        }

        if (!value.Contains("Encrypt=", StringComparison.OrdinalIgnoreCase))
        {
            value = value.TrimEnd(';') + ";Encrypt=True";
        }

        return value;
    }

    private static string StripKeyword(string value, string keyword)
    {
        var parts = value.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Where(part => !part.TrimStart().StartsWith($"{keyword}=", StringComparison.OrdinalIgnoreCase));

        return string.Join(';', parts);
    }
}
