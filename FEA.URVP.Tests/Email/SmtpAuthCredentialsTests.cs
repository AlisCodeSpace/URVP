using FEA.URVP.Application.Email;

namespace FEA.URVP.Tests.Email;

public sealed class SmtpAuthCredentialsTests
{
    [Fact]
    public void No_credentials_when_nothing_is_configured()
    {
        var auth = SmtpAuthCredentials.Resolve(null, null, null, null, "urvp-system@aub.edu.lb");

        Assert.Null(auth);
    }

    [Fact]
    public void Stored_password_wins_over_configuration()
    {
        var auth = SmtpAuthCredentials.Resolve(
            "config-user",
            "config-secret",
            "admin-user",
            "admin-secret",
            "urvp-system@aub.edu.lb");

        Assert.NotNull(auth);
        Assert.Equal("admin-user", auth.Value.UserName);
        Assert.Equal("admin-secret", auth.Value.Password);
    }

    [Fact]
    public void From_address_is_used_when_only_a_password_is_set()
    {
        var auth = SmtpAuthCredentials.Resolve(
            null,
            null,
            null,
            "mailbox-secret",
            "urvp-system@aub.edu.lb");

        Assert.NotNull(auth);
        Assert.Equal("urvp-system@aub.edu.lb", auth.Value.UserName);
        Assert.Equal("mailbox-secret", auth.Value.Password);
    }

    [Fact]
    public void Whitespace_is_ignored()
    {
        var auth = SmtpAuthCredentials.Resolve("  ", "  ", null, null, "urvp-system@aub.edu.lb");

        Assert.Null(auth);
    }
}
