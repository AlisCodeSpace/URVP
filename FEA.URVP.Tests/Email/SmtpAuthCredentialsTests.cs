using FEA.URVP.Application.Email;

namespace FEA.URVP.Tests.Email;

public sealed class SmtpAuthCredentialsTests
{
    [Fact]
    public void No_credentials_when_nothing_is_configured()
    {
        var auth = SmtpAuthCredentials.Resolve(null, null, null);

        Assert.Null(auth);
    }

    [Fact]
    public void Stored_password_wins_over_configuration()
    {
        var auth = SmtpAuthCredentials.Resolve(
            "config-user",
            "config-secret",
            "admin-secret");

        Assert.NotNull(auth);
        Assert.Equal("config-user", auth.Value.UserName);
        Assert.Equal("admin-secret", auth.Value.Password);
    }

    [Fact]
    public void Password_without_username_does_not_authenticate()
    {
        var auth = SmtpAuthCredentials.Resolve(
            null,
            null,
            "mailbox-secret");

        Assert.Null(auth);
    }

    [Fact]
    public void Username_without_password_does_not_authenticate()
    {
        var auth = SmtpAuthCredentials.Resolve("urvp-system", null, null);

        Assert.Null(auth);
    }

    [Fact]
    public void Whitespace_is_ignored()
    {
        var auth = SmtpAuthCredentials.Resolve("  ", "  ", null);

        Assert.Null(auth);
    }
}
