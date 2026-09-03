using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

// Microsoft.AspNetCore.HttpOverrides also declares an IPNetwork; the framework type is the one
// KnownIPNetworks accepts.
using IPNetwork = System.Net.IPNetwork;

namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Trusts <c>X-Forwarded-*</c> only from explicitly configured reverse proxies, so Scheme, Host
/// and client IP are the real public values used for OIDC redirects, cookie Secure decisions and
/// rate-limit partitioning.
/// </summary>
/// <remarks>
/// The default <c>KnownProxies</c>/<c>KnownNetworks</c> in ASP.NET Core are loopback only, and
/// clearing them entirely would let any caller spoof its own scheme and IP. Configure
/// <c>Security:TrustedProxies:KnownProxies</c> (or <c>KnownNetworks</c>) with the IIS or load
/// balancer address for every non-Development deployment.
/// </remarks>
public static class ForwardedHeadersConfiguration
{
    public static IServiceCollection AddForwardedHeadersSupport(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var trusted = configuration
            .GetSection(SecurityOptions.SectionName)
            .Get<SecurityOptions>()?.TrustedProxies
            ?? new TrustedProxyOptions();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost;

            options.ForwardLimit = trusted.ForwardLimit > 0 ? trusted.ForwardLimit : 1;

            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();

            if (trusted.TrustAnyProxy)
            {
                // Every address is trusted, for a PaaS host whose edge address is not knowable in
                // advance (for example Render).
                //
                // ForwardLimit is deliberately left at its configured value — normally 1 — rather
                // than being cleared. With no limit the middleware walks the whole X-Forwarded-For
                // chain, so a caller could prepend its own header and choose the IP the app sees;
                // that would hand an anonymous attacker a fresh rate-limit partition on every
                // request. Consuming a single hop takes only the rightmost entry, which is the one
                // the platform's edge appended, and leaves client-supplied entries ignored.
                options.KnownIPNetworks.Add(new IPNetwork(IPAddress.Any, 0));
                options.KnownIPNetworks.Add(new IPNetwork(IPAddress.IPv6Any, 0));
                return;
            }

            foreach (var address in IpAllowList.ParseAddresses(trusted.KnownProxies))
            {
                options.KnownProxies.Add(address);
            }

            foreach (var network in IpAllowList.ParseNetworks(trusted.KnownNetworks))
            {
                options.KnownIPNetworks.Add(network);
            }

            if (options.KnownProxies.Count == 0 && options.KnownIPNetworks.Count == 0)
            {
                // Loopback covers the IIS in-process/out-of-process reverse proxy on the same
                // host, which is the expected production topology.
                options.KnownProxies.Add(IPAddress.Loopback);
                options.KnownProxies.Add(IPAddress.IPv6Loopback);
            }
        });

        return services;
    }

    /// <summary>
    /// Fails fast in non-Development when nothing is configured and the deployment has not opted
    /// into trusting an unknown proxy, since silently trusting loopback only would break HTTPS
    /// detection behind a remote load balancer.
    /// </summary>
    public static void ValidateTrustedProxies(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        if (environment.IsDevelopment())
        {
            return;
        }

        var trusted = configuration
            .GetSection(SecurityOptions.SectionName)
            .Get<SecurityOptions>()?.TrustedProxies
            ?? new TrustedProxyOptions();

        if (trusted.TrustAnyProxy)
        {
            logger.LogWarning(
                "Security:TrustedProxies:TrustAnyProxy is enabled. Forwarded headers are accepted "
                + "from any address. Only use this on a platform that strips client-supplied "
                + "X-Forwarded-* headers.");
            return;
        }

        if (trusted.KnownProxies.Count == 0 && trusted.KnownNetworks.Count == 0)
        {
            logger.LogWarning(
                "Security:TrustedProxies is not configured. Only loopback proxies are trusted, "
                + "which is correct for IIS on the same host but will break scheme and client-IP "
                + "detection behind a remote load balancer.");
        }
    }
}
