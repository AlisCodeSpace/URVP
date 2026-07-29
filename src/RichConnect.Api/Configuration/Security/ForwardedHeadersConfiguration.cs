using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace RICHConnect.Backend.Api.Configuration.Security
{
    /// <summary>
    /// Configuration for forwarded headers middleware
    /// Required when running behind a reverse proxy (IIS, Nginx, Azure, etc.)
    /// to ensure rate limiting and logging see the real client IP, not the proxy IP
    /// </summary>
    public static class ForwardedHeadersConfiguration
    {
        /// <summary>
        /// Configure forwarded headers services
        /// </summary>
        public static IServiceCollection AddForwardedHeadersConfiguration(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                // Forward X-Forwarded-For and X-Forwarded-Proto headers
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                var knownProxies = configuration.GetSection("ForwardedHeaders:KnownProxies")
                    .Get<string[]>() ?? Array.Empty<string>();
                var knownNetworks = configuration.GetSection("ForwardedHeaders:KnownNetworks")
                    .Get<string[]>() ?? Array.Empty<string>();
                var forwardLimit = configuration.GetValue<int?>("ForwardedHeaders:ForwardLimit");

                // Limit the number of proxies to prevent header injection attacks
                options.ForwardLimit = forwardLimit ?? 2;

                // Only trust explicit proxies/networks. If none are configured, keep framework defaults
                // (loopback only) to avoid trusting spoofed headers in production.
                if (knownProxies.Length > 0 || knownNetworks.Length > 0)
                {
                    options.KnownProxies.Clear();
                    options.KnownIPNetworks.Clear();

                    foreach (var proxy in knownProxies)
                    {
                        if (!IPAddress.TryParse(proxy, out var ipAddress))
                        {
                            throw new InvalidOperationException(
                                $"ForwardedHeaders:KnownProxies contains invalid IP address '{proxy}'.");
                        }

                        options.KnownProxies.Add(ipAddress);
                    }

                    foreach (var network in knownNetworks)
                    {
                        if (!System.Net.IPNetwork.TryParse(network, out var ipNetwork))
                        {
                            throw new InvalidOperationException(
                                $"ForwardedHeaders:KnownNetworks contains invalid CIDR '{network}'. Expected format: 10.0.0.0/8.");
                        }

                        options.KnownIPNetworks.Add(ipNetwork);
                    }
                }
            });

            return services;
        }
    }
}
