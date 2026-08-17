using Microsoft.AspNetCore.HttpOverrides;

namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Trust platform reverse-proxy headers (Render, Azure, etc.) so Scheme/Host
/// are the public HTTPS values used for OIDC redirects and cookies.
/// </summary>
public static class ForwardedHeadersConfiguration
{
    public static IServiceCollection AddForwardedHeadersSupport(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor
                | ForwardedHeaders.XForwardedProto
                | ForwardedHeaders.XForwardedHost;
            options.ForwardLimit = 1;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }
}
