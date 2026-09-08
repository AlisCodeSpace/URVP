using FEA.URVP.Infrastructure.Security;
using Microsoft.AspNetCore.DataProtection;

namespace FEA.URVP.Tests.Email;

public sealed class DataProtectionSmtpCredentialProtectorTests
{
    [Fact]
    public void Protect_round_trips_and_is_not_plaintext()
    {
        const string secret = "mailbox-secret";
        var protector = CreateProtector();

        var protectedPayload = protector.Protect(secret);

        Assert.NotEqual(secret, protectedPayload);
        Assert.DoesNotContain(secret, protectedPayload, StringComparison.Ordinal);
        Assert.True(protector.TryUnprotect(protectedPayload, out var plaintext));
        Assert.Equal(secret, plaintext);
    }

    [Fact]
    public void Tampered_payload_does_not_unprotect()
    {
        var protector = CreateProtector();
        var protectedPayload = protector.Protect("mailbox-secret");

        Assert.False(protector.TryUnprotect(protectedPayload + "x", out var plaintext));
        Assert.Null(plaintext);
    }

    [Fact]
    public void Empty_payload_does_not_unprotect()
    {
        var protector = CreateProtector();

        Assert.False(protector.TryUnprotect(string.Empty, out var plaintext));
        Assert.Null(plaintext);
    }

    private static DataProtectionSmtpCredentialProtector CreateProtector() =>
        new(new EphemeralDataProtectionProvider());
}
