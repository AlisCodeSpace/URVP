using System.Net;
using FEA.URVP.Api.Configuration.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Tests.Security;

/// <summary>
/// Forwarded-header trust decides the scheme, the host used for OIDC redirects, and the client IP
/// the rate limiter partitions on, so a mistake here silently disables rate limiting.
/// </summary>
public sealed class ForwardedHeadersConfigurationTests
{
    private static ForwardedHeadersOptions Resolve(params (string Key, string? Value)[] values) =>
        new ServiceCollection()
            .AddForwardedHeadersSupport(TestEnvironments.Config(values))
            .BuildServiceProvider()
            .GetRequiredService<IOptions<ForwardedHeadersOptions>>()
            .Value;

    [Fact]
    public void Scheme_host_and_client_ip_are_all_forwarded()
    {
        var options = Resolve();

        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor));
        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto));
        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost));
    }

    [Fact]
    public void Nothing_configured_trusts_loopback_only()
    {
        // Correct for IIS on the same host, and it must never silently widen to everything.
        var options = Resolve();

        Assert.Empty(options.KnownIPNetworks);
        Assert.Equal(2, options.KnownProxies.Count);
        Assert.All(options.KnownProxies, address => Assert.True(IPAddress.IsLoopback(address)));
    }

    [Fact]
    public void An_explicit_proxy_replaces_the_loopback_default()
    {
        var options = Resolve(("Security:TrustedProxies:KnownProxies:0", "10.4.1.7"));

        Assert.Contains(IPAddress.Parse("10.4.1.7"), options.KnownProxies);
        Assert.Single(options.KnownProxies);
    }

    [Fact]
    public void Trust_any_proxy_still_consumes_only_one_hop()
    {
        // The regression this guards: clearing ForwardLimit makes the middleware walk the whole
        // X-Forwarded-For chain, so any caller can prepend a header and pick the IP the app sees.
        // That would give an anonymous attacker a fresh rate-limit partition per request.
        var options = Resolve(("Security:TrustedProxies:TrustAnyProxy", "true"));

        Assert.Equal(1, options.ForwardLimit);
        Assert.Equal(2, options.KnownIPNetworks.Count);
    }

    [Fact]
    public void Trust_any_proxy_honours_an_explicit_hop_count()
    {
        var options = Resolve(
            ("Security:TrustedProxies:TrustAnyProxy", "true"),
            ("Security:TrustedProxies:ForwardLimit", "2"));

        Assert.Equal(2, options.ForwardLimit);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void A_nonsensical_hop_count_falls_back_to_one(string configured)
    {
        var options = Resolve(("Security:TrustedProxies:ForwardLimit", configured));

        Assert.Equal(1, options.ForwardLimit);
    }
}
