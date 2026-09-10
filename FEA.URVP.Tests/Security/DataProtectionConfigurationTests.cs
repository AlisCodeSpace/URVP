using FEA.URVP.Infrastructure.DataProtection;

namespace FEA.URVP.Tests.Security;

public sealed class DataProtectionConfigurationTests
{
    [Fact]
    public void Blank_thumbprint_is_treated_as_unset()
    {
        var configuration = TestEnvironments.Config(
            (DataProtectionConfiguration.CertificateThumbprintKey, "  "));

        Assert.Null(DataProtectionConfiguration.ReadCertificateThumbprint(configuration));
    }

    [Fact]
    public void Configured_thumbprint_is_trimmed()
    {
        var configuration = TestEnvironments.Config(
            (DataProtectionConfiguration.CertificateThumbprintKey, "  ABC123  "));

        Assert.Equal("ABC123", DataProtectionConfiguration.ReadCertificateThumbprint(configuration));
    }

    [Fact]
    public void Windows_encrypts_keys_at_rest_without_a_certificate()
    {
        Assert.Equal(
            OperatingSystem.IsWindows(),
            DataProtectionConfiguration.EncryptsKeysAtRest(TestEnvironments.Config()));
    }

    [Fact]
    public void A_certificate_encrypts_keys_on_any_platform()
    {
        var configuration = TestEnvironments.Config(
            (DataProtectionConfiguration.CertificateThumbprintKey, "ABC123"));

        Assert.True(DataProtectionConfiguration.EncryptsKeysAtRest(configuration));
    }

    [Fact]
    public void An_encryption_secret_encrypts_keys_on_any_platform()
    {
        var configuration = TestEnvironments.Config(
            (DataProtectionConfiguration.EncryptionKeySecretKey, "render-generated-secret"));

        Assert.Equal("render-generated-secret", DataProtectionConfiguration.ReadEncryptionSecret(configuration));
        Assert.True(DataProtectionConfiguration.EncryptsKeysAtRest(configuration));
    }
}
