using System.Net;
using FEA.URVP.Api.Configuration.Security;

namespace FEA.URVP.Tests.Security;

public sealed class IpAllowListTests
{
    [Fact]
    public void ParseAddresses_reads_valid_addresses_and_reports_the_rest()
    {
        var invalid = new List<string>();

        var addresses = IpAllowList.ParseAddresses(
            ["10.1.2.3", " ::1 ", "not-an-ip", ""],
            invalid.Add);

        Assert.Equal(2, addresses.Count);
        Assert.Equal(["not-an-ip"], invalid);
    }

    [Fact]
    public void ParseNetworks_accepts_cidr_and_treats_a_bare_address_as_a_single_host()
    {
        var networks = IpAllowList.ParseNetworks(["10.0.0.0/8", "192.168.1.50"]);

        Assert.Equal(2, networks.Count);
        Assert.True(IpAllowList.Contains(networks, IPAddress.Parse("10.44.1.9")));
        Assert.True(IpAllowList.Contains(networks, IPAddress.Parse("192.168.1.50")));
        Assert.False(IpAllowList.Contains(networks, IPAddress.Parse("192.168.1.51")));
    }

    [Fact]
    public void ParseNetworks_reports_unparseable_entries_instead_of_throwing()
    {
        var invalid = new List<string>();

        var networks = IpAllowList.ParseNetworks(["10.0.0.0/8", "10.0.0.0/999"], invalid.Add);

        Assert.Single(networks);
        Assert.Equal(["10.0.0.0/999"], invalid);
    }

    [Fact]
    public void Contains_matches_an_ipv4_mapped_ipv6_client_against_an_ipv4_range()
    {
        // A dual-stack socket surfaces an IPv4 client as ::ffff:a.b.c.d.
        var networks = IpAllowList.ParseNetworks(["10.0.0.0/8"]);
        var mapped = IPAddress.Parse("10.5.6.7").MapToIPv6();

        Assert.True(mapped.IsIPv4MappedToIPv6);
        Assert.True(IpAllowList.Contains(networks, mapped));
    }

    [Fact]
    public void Contains_rejects_addresses_outside_every_range()
    {
        var networks = IpAllowList.ParseNetworks(["10.0.0.0/8"]);

        Assert.False(IpAllowList.Contains(networks, IPAddress.Parse("203.0.113.9")));
    }

    [Fact]
    public void Contains_is_false_for_a_null_address_or_an_empty_list()
    {
        Assert.False(IpAllowList.Contains(IpAllowList.ParseNetworks(["10.0.0.0/8"]), null));
        Assert.False(IpAllowList.Contains([], IPAddress.Loopback));
    }

    [Fact]
    public void Empty_configuration_parses_to_nothing()
    {
        Assert.Empty(IpAllowList.ParseAddresses(null));
        Assert.Empty(IpAllowList.ParseNetworks(null));
    }
}
