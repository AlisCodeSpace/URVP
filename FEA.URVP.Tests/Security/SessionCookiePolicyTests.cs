using FEA.URVP.Api.Configuration.Security;
using Microsoft.AspNetCore.Http;

namespace FEA.URVP.Tests.Security;

public sealed class SessionCookiePolicyTests
{
    [Fact]
    public void Production_is_strict_and_secure_regardless_of_configuration()
    {
        // Even with a split-origin CORS configuration present, Production must not relax.
        var configuration = TestEnvironments.WithCorsOrigins("https://portal.example.edu");

        var (sameSite, secure) = SessionCookiePolicy.Resolve(
            configuration,
            TestEnvironments.Production);

        Assert.Equal(SameSiteMode.Strict, sameSite);
        Assert.Equal(CookieSecurePolicy.Always, secure);
    }

    [Fact]
    public void Staging_is_treated_as_production()
    {
        var (sameSite, secure) = SessionCookiePolicy.Resolve(
            TestEnvironments.Config(),
            TestEnvironments.Staging);

        Assert.Equal(SameSiteMode.Strict, sameSite);
        Assert.Equal(CookieSecurePolicy.Always, secure);
    }

    [Fact]
    public void Development_split_origin_relaxes_to_none_but_keeps_secure()
    {
        var configuration = TestEnvironments.WithCorsOrigins("https://localhost:3000");

        var (sameSite, secure) = SessionCookiePolicy.Resolve(
            configuration,
            TestEnvironments.Development);

        Assert.Equal(SameSiteMode.None, sameSite);
        Assert.Equal(CookieSecurePolicy.Always, secure);
    }

    [Fact]
    public void Development_same_origin_uses_lax()
    {
        var (sameSite, _) = SessionCookiePolicy.Resolve(
            TestEnvironments.Config(),
            TestEnvironments.Development);

        Assert.Equal(SameSiteMode.Lax, sameSite);
    }

    [Theory]
    [InlineData("Strict", SameSiteMode.Strict)]
    [InlineData("strict", SameSiteMode.Strict)]
    [InlineData("  lax  ", SameSiteMode.Lax)]
    [InlineData("None", SameSiteMode.None)]
    [InlineData("Unspecified", SameSiteMode.Unspecified)]
    public void ParseSameSiteMode_accepts_known_values(string input, SameSiteMode expected)
    {
        Assert.Equal(expected, SessionCookiePolicy.ParseSameSiteMode(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("relaxed")]
    public void ParseSameSiteMode_rejects_unknown_values(string? input)
    {
        Assert.Null(SessionCookiePolicy.ParseSameSiteMode(input));
    }
}
