using System.Text.RegularExpressions;

namespace FEA.URVP.Api.Middleware;

/// <summary>
/// Masks credential-shaped substrings before text derived from requests, exceptions or
/// browser-submitted reports is written to a log sink.
/// </summary>
/// <remarks>
/// This is a safety net, not a licence to log secrets deliberately. Call sites should still avoid
/// passing cookies, tokens and authorization headers to the logger in the first place.
/// </remarks>
public static partial class SecretRedactor
{
    private const string Mask = "[redacted]";

    public static string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        // Token patterns run first. The key/value pattern would otherwise consume the "Bearer"
        // keyword as if it were the value of an "Authorization:" key, leaving the token itself
        // in the string.
        var redacted = BearerTokenRegex().Replace(value, $"Bearer {Mask}");
        redacted = JwtRegex().Replace(redacted, Mask);
        redacted = KeyValueSecretRegex().Replace(redacted, $"$1={Mask}");

        return redacted;
    }

    /// <summary>
    /// Redacts and hard-truncates untrusted input so a large or hostile payload cannot flood or
    /// corrupt the log.
    /// </summary>
    public static string RedactAndTruncate(string? value, int maxLength)
    {
        var redacted = Redact(value);

        // Control characters would let a report body forge extra log lines.
        redacted = ControlCharacterRegex().Replace(redacted, " ");

        return redacted.Length <= maxLength
            ? redacted
            : string.Concat(redacted.AsSpan(0, maxLength), "...[truncated]");
    }

    [GeneratedRegex(
        @"\b(password|pwd|client_secret|clientsecret|secret|access_token|id_token|refresh_token|api[-_]?key|apikey|accountkey|authorization|code_verifier)\b\s*[=:]\s*""?[^""&;,\s]+""?",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex KeyValueSecretRegex();

    [GeneratedRegex(@"Bearer\s+[A-Za-z0-9\-._~+/]+=*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"\beyJ[A-Za-z0-9_-]{8,}\.[A-Za-z0-9_-]{8,}\.[A-Za-z0-9_-]+", RegexOptions.CultureInvariant)]
    private static partial Regex JwtRegex();

    [GeneratedRegex(@"[\p{Cc}\p{Cf}]", RegexOptions.CultureInvariant)]
    private static partial Regex ControlCharacterRegex();
}
