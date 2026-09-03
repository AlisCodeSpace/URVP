using System.Security.Cryptography;

namespace FEA.URVP.Api.Middleware;

/// <summary>
/// Per-response Content-Security-Policy nonce, shared between the component that injects it into
/// the exported HTML and the component that writes the CSP header.
/// </summary>
public static class CspNonce
{
    private const string ItemKey = "Security:CspNonce";

    /// <summary>
    /// Returns this response's nonce, creating one on first use. A response must never reuse a
    /// nonce, so the value is cached on <see cref="HttpContext.Items"/> rather than regenerated.
    /// </summary>
    public static string Get(HttpContext context)
    {
        if (context.Items.TryGetValue(ItemKey, out var existing) && existing is string nonce)
        {
            return nonce;
        }

        var generated = Create();
        context.Items[ItemKey] = generated;
        return generated;
    }

    /// <summary>
    /// Returns the nonce only if one was already created for this response.
    /// </summary>
    public static string? Peek(HttpContext context) =>
        context.Items.TryGetValue(ItemKey, out var existing) && existing is string nonce
            ? nonce
            : null;

    private static string Create() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
}
