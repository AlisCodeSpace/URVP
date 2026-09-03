using FEA.URVP.Api.Configuration.Auth;

namespace FEA.URVP.Tests.Security;

public sealed class DevSignInPolicyTests
{
    [Theory]
    [InlineData("true")]
    [InlineData("True")]
    [InlineData(null)]
    public void Production_never_enables_demo_sign_in(string? configured)
    {
        // The demo endpoint mints a privileged session from an email address alone, so no
        // configuration value may switch it on in Production.
        var configuration = TestEnvironments.Config(
            (DevSignInPolicy.ConfigurationKey, configured));

        Assert.False(DevSignInPolicy.IsEnabled(configuration, TestEnvironments.Production));
    }

    [Fact]
    public void Development_enables_demo_sign_in_by_default()
    {
        Assert.True(DevSignInPolicy.IsEnabled(
            TestEnvironments.Config(),
            TestEnvironments.Development));
    }

    [Fact]
    public void Development_honours_an_explicit_opt_out()
    {
        var configuration = TestEnvironments.Config(
            (DevSignInPolicy.ConfigurationKey, "false"));

        Assert.False(DevSignInPolicy.IsEnabled(configuration, TestEnvironments.Development));
    }

    [Fact]
    public void Staging_requires_an_explicit_opt_in()
    {
        Assert.False(DevSignInPolicy.IsEnabled(
            TestEnvironments.Config(),
            TestEnvironments.Staging));

        var optedIn = TestEnvironments.Config((DevSignInPolicy.ConfigurationKey, "true"));

        Assert.True(DevSignInPolicy.IsEnabled(optedIn, TestEnvironments.Staging));
    }
}
