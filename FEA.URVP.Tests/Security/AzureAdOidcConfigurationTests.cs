using FEA.URVP.Api.Configuration.Auth;
using Microsoft.Extensions.Configuration;

namespace FEA.URVP.Tests.Security;

/// <summary>
/// Guards the registration precondition for the OIDC scheme.
/// </summary>
/// <remarks>
/// The handler implements <c>IAuthenticationRequestHandler</c>, so ASP.NET Core builds its
/// options on every request to test for the callback path. A missing tenant or client id
/// discovered inside the options factory therefore fails every route in the application,
/// <c>/health/live</c> included, and only at request time — long after startup reported success.
/// Callers must be able to detect the gap before the scheme is registered.
/// </remarks>
public sealed class AzureAdOidcConfigurationTests
{
    private static IConfiguration Complete() => TestEnvironments.Config(
        ("AzureAd:TenantId", "00000000-1111-2222-3333-444444444444"),
        ("AzureAd:ClientId", "11111111-2222-3333-4444-555555555555"));

    [Fact]
    public void A_complete_configuration_is_registrable()
    {
        Assert.True(AzureAdOidcConfiguration.IsConfigured(Complete()));
        Assert.Empty(AzureAdOidcConfiguration.MissingSettings(Complete()));
    }

    [Fact]
    public void An_empty_configuration_names_every_missing_setting()
    {
        var missing = AzureAdOidcConfiguration.MissingSettings(TestEnvironments.Config());

        Assert.False(AzureAdOidcConfiguration.IsConfigured(TestEnvironments.Config()));
        Assert.Equal(["AzureAd:TenantId", "AzureAd:ClientId"], missing);
    }

    [Theory]
    [InlineData("AzureAd:TenantId")]
    [InlineData("AzureAd:ClientId")]
    public void A_single_missing_setting_is_reported_on_its_own(string key)
    {
        var values = new List<(string, string?)>
        {
            ("AzureAd:TenantId", "00000000-1111-2222-3333-444444444444"),
            ("AzureAd:ClientId", "11111111-2222-3333-4444-555555555555")
        };
        values.RemoveAll(pair => pair.Item1 == key);

        var configuration = TestEnvironments.Config(values.ToArray());

        Assert.False(AzureAdOidcConfiguration.IsConfigured(configuration));
        Assert.Equal([key], AzureAdOidcConfiguration.MissingSettings(configuration));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_blank_setting_counts_as_missing(string blank)
    {
        // An environment variable set to the empty string is a common deployment mistake and
        // must not be mistaken for a configured value.
        var configuration = TestEnvironments.Config(
            ("AzureAd:TenantId", blank),
            ("AzureAd:ClientId", "11111111-2222-3333-4444-555555555555"));

        Assert.False(AzureAdOidcConfiguration.IsConfigured(configuration));
        Assert.Equal(["AzureAd:TenantId"], AzureAdOidcConfiguration.MissingSettings(configuration));
    }

    [Fact]
    public void A_client_secret_is_not_required()
    {
        // Without a secret the handler falls back to id_token/form_post, which is a documented
        // and supported topology; it must not block registration.
        Assert.True(AzureAdOidcConfiguration.IsConfigured(Complete()));
    }
}
