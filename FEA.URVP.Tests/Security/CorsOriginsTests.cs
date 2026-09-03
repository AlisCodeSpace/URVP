using FEA.URVP.Api.Configuration.Security;

namespace FEA.URVP.Tests.Security;

public sealed class CorsOriginsTests
{
    [Fact]
    public void Https_origins_are_accepted()
    {
        var configuration = TestEnvironments.WithCorsOrigins(
            "https://urvp.aub.edu.lb",
            "https://www.urvp.aub.edu.lb");

        var accepted = CorsOrigins.GetAllowedOrigins(configuration);

        Assert.Equal(
            ["https://urvp.aub.edu.lb", "https://www.urvp.aub.edu.lb"],
            accepted);
    }

    [Theory]
    [InlineData("*")]
    [InlineData("https://*.aub.edu.lb")]
    [InlineData("http://urvp.aub.edu.lb")]
    [InlineData("https://urvp.aub.edu.lb/portal")]
    [InlineData("https://urvp.aub.edu.lb?x=1")]
    [InlineData("urvp.aub.edu.lb")]
    [InlineData("not a url")]
    public void Wildcards_plain_http_and_malformed_origins_are_rejected(string origin)
    {
        var configuration = TestEnvironments.WithCorsOrigins(origin);

        Assert.Empty(CorsOrigins.GetAllowedOrigins(configuration));
        Assert.Contains(origin.TrimEnd('/'), CorsOrigins.GetRejectedOrigins(configuration));
    }

    [Fact]
    public void Plain_http_loopback_is_accepted_only_when_explicitly_allowed()
    {
        var configuration = TestEnvironments.WithCorsOrigins("http://localhost:3000");

        Assert.Empty(CorsOrigins.GetAllowedOrigins(configuration));
        Assert.Single(CorsOrigins.GetAllowedOrigins(configuration, allowInsecureLoopback: true));
    }

    [Fact]
    public void Non_loopback_plain_http_stays_rejected_even_for_development()
    {
        var configuration = TestEnvironments.WithCorsOrigins("http://staging.example.edu");

        Assert.Empty(CorsOrigins.GetAllowedOrigins(configuration, allowInsecureLoopback: true));
    }

    [Fact]
    public void Comma_separated_environment_variable_form_is_supported()
    {
        var configuration = TestEnvironments.Config(
            ("Cors:AllowedOrigins", "https://a.example.edu, https://b.example.edu"));

        Assert.Equal(
            ["https://a.example.edu", "https://b.example.edu"],
            CorsOrigins.GetAllowedOrigins(configuration));
    }

    [Fact]
    public void Trailing_slashes_and_duplicates_are_normalized_away()
    {
        var configuration = TestEnvironments.WithCorsOrigins(
            "https://urvp.aub.edu.lb/",
            "https://URVP.aub.edu.lb");

        Assert.Single(CorsOrigins.GetAllowedOrigins(configuration));
    }

    [Fact]
    public void No_configuration_yields_no_allowed_origins()
    {
        // The CORS policy treats this as "deny every cross-origin request".
        Assert.Empty(CorsOrigins.GetAllowedOrigins(TestEnvironments.Config()));
    }
}
