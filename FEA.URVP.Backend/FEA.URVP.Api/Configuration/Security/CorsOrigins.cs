namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Reads CORS origins from configuration. Supports a JSON array
/// (<c>Cors:AllowedOrigins:0</c>) or a comma-separated string
/// (<c>Cors__AllowedOrigins</c>) for PaaS environment variables.
/// </summary>
public static class CorsOrigins
{
    public static string[] GetAllowedOrigins(IConfiguration configuration)
    {
        var section = configuration.GetSection("Cors:AllowedOrigins");
        var fromArray = section.Get<string[]>();
        if (fromArray is { Length: > 0 })
        {
            return Normalize(fromArray);
        }

        var fromString = section.Value ?? configuration["Cors:AllowedOrigins"];
        if (string.IsNullOrWhiteSpace(fromString))
        {
            return [];
        }

        return Normalize(fromString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static string[] Normalize(IEnumerable<string?> origins) =>
        origins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin!.Trim().TrimEnd('/'))
            .Where(origin => origin.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
