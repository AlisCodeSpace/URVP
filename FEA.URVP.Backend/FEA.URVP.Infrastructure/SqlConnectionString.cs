namespace FEA.URVP.Infrastructure;

/// <summary>
/// Normalizes SQL Server connection strings from PaaS dashboards.
/// Hosts like MonsterASP often copy as <c>host;Database=...</c> without <c>Server=</c>,
/// which SqlClient rejects as keyword <c>host;database</c>.
/// </summary>
public static class SqlConnectionString
{
    public static string Normalize(string connectionString)
    {
        var value = connectionString.Trim().Trim('"').Trim('\'');
        if (value.Length == 0)
        {
            return value;
        }

        if (HasServerKeyword(value))
        {
            return EnsureMultipleActiveResultSets(value);
        }

        return EnsureMultipleActiveResultSets($"Server={value}");
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
}
