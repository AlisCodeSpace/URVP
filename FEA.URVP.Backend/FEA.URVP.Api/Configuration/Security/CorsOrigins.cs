namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Reads and validates CORS origins from configuration. Supports a JSON array
/// (<c>Cors:AllowedOrigins:0</c>) or a comma-separated string (<c>Cors__AllowedOrigins</c>) for
/// PaaS environment variables.
/// </summary>
/// <remarks>
/// Also the single source of truth for the post-login return-URL allow-list, so an origin can
/// never be trusted for redirects without also being an accepted browser origin.
/// </remarks>
public static class CorsOrigins
{
    /// <summary>
    /// Origins that passed validation. Anything not an absolute, wildcard-free HTTPS origin is
    /// dropped, except plain-HTTP loopback when <paramref name="allowInsecureLoopback"/> is set
    /// for local development.
    /// </summary>
    public static string[] GetAllowedOrigins(
        IConfiguration configuration,
        bool allowInsecureLoopback = false) =>
        Partition(configuration, allowInsecureLoopback).Accepted;

    /// <summary>
    /// Configured origins that were rejected, for startup diagnostics.
    /// </summary>
    public static string[] GetRejectedOrigins(
        IConfiguration configuration,
        bool allowInsecureLoopback = false) =>
        Partition(configuration, allowInsecureLoopback).Rejected;

    private static (string[] Accepted, string[] Rejected) Partition(
        IConfiguration configuration,
        bool allowInsecureLoopback)
    {
        var configured = ReadConfiguredValues(configuration);
        var accepted = new List<string>();
        var rejected = new List<string>();

        foreach (var origin in configured)
        {
            if (IsValidOrigin(origin, allowInsecureLoopback))
            {
                accepted.Add(origin);
            }
            else
            {
                rejected.Add(origin);
            }
        }

        return (
            accepted.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            rejected.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static string[] ReadConfiguredValues(IConfiguration configuration)
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

    private static bool IsValidOrigin(string origin, bool allowInsecureLoopback)
    {
        // A wildcard combined with AllowCredentials is rejected by browsers anyway, but it must
        // never reach the policy builder, which would throw at startup.
        if (origin.Contains('*'))
        {
            return false;
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        // An origin is scheme + host + port only; a path or query means the value is malformed
        // and would silently never match.
        if (uri.AbsolutePath is not ("" or "/") || uri.Query.Length > 0 || uri.Fragment.Length > 0)
        {
            return false;
        }

        if (uri.Scheme == Uri.UriSchemeHttps)
        {
            return true;
        }

        return allowInsecureLoopback
            && uri.Scheme == Uri.UriSchemeHttp
            && uri.IsLoopback;
    }

    private static string[] Normalize(IEnumerable<string?> origins) =>
        origins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin!.Trim().TrimEnd('/'))
            .Where(origin => origin.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
