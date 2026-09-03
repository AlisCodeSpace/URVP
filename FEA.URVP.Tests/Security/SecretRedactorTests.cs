using FEA.URVP.Api.Middleware;

namespace FEA.URVP.Tests.Security;

public sealed class SecretRedactorTests
{
    [Theory]
    [InlineData("Server=db;Password=Sup3rSecret!;Encrypt=True", "Sup3rSecret!")]
    [InlineData("client_secret=abc123def", "abc123def")]
    [InlineData("ClientSecret: \"abc123def\"", "abc123def")]
    [InlineData("AccountKey=Zm9vYmFyYmF6", "Zm9vYmFyYmF6")]
    [InlineData("api_key = 9f8e7d6c5b4a", "9f8e7d6c5b4a")]
    [InlineData("code_verifier=dBjftJeZ4CVPmB92K27uhbUJU1p1r", "dBjftJeZ4CVPmB92K27uhbUJU1p1r")]
    public void Redact_removes_key_value_secrets(string input, string secret)
    {
        var redacted = SecretRedactor.Redact(input);

        Assert.DoesNotContain(secret, redacted, StringComparison.Ordinal);
        Assert.Contains("[redacted]", redacted, StringComparison.Ordinal);
    }

    [Fact]
    public void Redact_removes_bearer_tokens()
    {
        var redacted = SecretRedactor.Redact("Authorization: Bearer abc.def.ghi123");

        Assert.DoesNotContain("abc.def.ghi123", redacted, StringComparison.Ordinal);
    }

    [Fact]
    public void Redact_removes_jwt_shaped_values()
    {
        const string jwt = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dBjftJeZ4CVP";

        var redacted = SecretRedactor.Redact($"id_token was {jwt} at sign-in");

        Assert.DoesNotContain(jwt, redacted, StringComparison.Ordinal);
    }

    [Fact]
    public void Redact_null_or_empty_returns_empty()
    {
        Assert.Equal(string.Empty, SecretRedactor.Redact(null));
        Assert.Equal(string.Empty, SecretRedactor.Redact(string.Empty));
    }

    [Fact]
    public void Redact_leaves_ordinary_text_untouched()
    {
        const string message = "Readiness probe: database connectivity check failed.";

        Assert.Equal(message, SecretRedactor.Redact(message));
    }

    [Fact]
    public void RedactAndTruncate_caps_length()
    {
        var redacted = SecretRedactor.RedactAndTruncate(new string('a', 5_000), 100);

        Assert.StartsWith(new string('a', 100), redacted, StringComparison.Ordinal);
        Assert.EndsWith("...[truncated]", redacted, StringComparison.Ordinal);
    }

    [Fact]
    public void RedactAndTruncate_strips_control_characters_that_could_forge_log_lines()
    {
        var redacted = SecretRedactor.RedactAndTruncate(
            "csp-report\r\nWARN  Administrator signed in",
            2000);

        Assert.DoesNotContain('\r', redacted);
        Assert.DoesNotContain('\n', redacted);
    }

    [Fact]
    public void RedactAndTruncate_still_redacts_secrets()
    {
        var redacted = SecretRedactor.RedactAndTruncate("password=hunter2", 2000);

        Assert.DoesNotContain("hunter2", redacted, StringComparison.Ordinal);
    }
}
