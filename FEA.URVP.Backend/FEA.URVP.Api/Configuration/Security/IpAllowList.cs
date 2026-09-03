using System.Net;

namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Parses IP and CIDR allow-lists from configuration. Shared by the trusted-proxy configuration
/// and the health monitoring allow-list so both accept the same syntax.
/// </summary>
public static class IpAllowList
{
    /// <summary>
    /// Parses bare addresses such as <c>10.1.2.3</c>. Unparseable entries are skipped and
    /// reported through <paramref name="onInvalid"/> rather than failing startup, so one bad
    /// environment variable cannot take the site down.
    /// </summary>
    public static IReadOnlyList<IPAddress> ParseAddresses(
        IEnumerable<string>? values,
        Action<string>? onInvalid = null)
    {
        var addresses = new List<IPAddress>();

        foreach (var value in Clean(values))
        {
            if (IPAddress.TryParse(value, out var address))
            {
                addresses.Add(address);
            }
            else
            {
                onInvalid?.Invoke(value);
            }
        }

        return addresses;
    }

    /// <summary>
    /// Parses CIDR ranges such as <c>10.0.0.0/8</c>. A bare address is accepted and treated as a
    /// single-host network.
    /// </summary>
    public static IReadOnlyList<IPNetwork> ParseNetworks(
        IEnumerable<string>? values,
        Action<string>? onInvalid = null)
    {
        var networks = new List<IPNetwork>();

        foreach (var value in Clean(values))
        {
            if (IPNetwork.TryParse(value, out var network))
            {
                networks.Add(network);
                continue;
            }

            if (IPAddress.TryParse(value, out var address))
            {
                networks.Add(new IPNetwork(address, address.GetAddressBytes().Length * 8));
                continue;
            }

            onInvalid?.Invoke(value);
        }

        return networks;
    }

    public static bool Contains(IEnumerable<IPNetwork> networks, IPAddress? address)
    {
        if (address is null)
        {
            return false;
        }

        // An IPv4 client arriving over a dual-stack socket is surfaced as ::ffff:a.b.c.d, which
        // never matches an IPv4 CIDR entry unless it is unmapped first.
        var candidate = address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;

        return networks.Any(network => network.Contains(candidate) || network.Contains(address));
    }

    private static IEnumerable<string> Clean(IEnumerable<string>? values) =>
        values?
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
        ?? [];
}
