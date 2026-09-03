using System.Text;

namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Builds the Content-Security-Policy header values.
/// </summary>
/// <remarks>
/// Two policies exist. The document policy covers HTML served for the exported Next.js app and
/// carries a per-response nonce, because the prerendered HTML contains Next.js bootstrap and
/// flight-data inline scripts. The API policy is stricter: API and file responses are never a
/// script host, so everything is denied by default.
/// </remarks>
public static class ContentSecurityPolicy
{
    private const string Self = "'self'";
    private const string None = "'none'";

    public static string BuildForDocument(ContentSecurityPolicyOptions options, string nonce)
    {
        var policy = new StringBuilder();

        Append(policy, "default-src", Self);
        Append(policy, "base-uri", Self);
        Append(policy, "object-src", None);

        // 'self' covers the hashed /_next/static bundles; the nonce covers Next.js inline
        // bootstrap. 'unsafe-inline' is deliberately absent, and modern browsers ignore it
        // whenever a nonce is present anyway.
        Append(policy, "script-src", $"{Self} 'nonce-{nonce}'");
        Append(policy, "script-src-attr", None);

        // Radix Themes and Tailwind emit inline style attributes at runtime, so style-src
        // cannot drop 'unsafe-inline'. Documented as an accepted risk: inline styles cannot
        // execute script while script-src-attr is 'none' and object-src is 'none'.
        Append(policy, "style-src", $"{Self} 'unsafe-inline'");

        // data: is required by the inline SVG background-image in globals.css.
        Append(policy, "img-src", Combine($"{Self} data:", options.ImgSrc));
        Append(policy, "font-src", Combine(Self, options.FontSrc));
        Append(policy, "connect-src", Combine(Self, options.ConnectSrc));
        Append(policy, "manifest-src", Self);
        Append(policy, "worker-src", Self);
        Append(policy, "media-src", Self);
        Append(policy, "frame-src", ListOrNone(options.FrameSrc));
        Append(policy, "frame-ancestors", ListOrNone(options.FrameAncestors));
        Append(policy, "form-action", Combine(Self, options.FormAction));
        Append(policy, "upgrade-insecure-requests", null);

        AppendReport(policy, options.ReportPath);

        return policy.ToString();
    }

    public static string BuildForApi(ContentSecurityPolicyOptions options)
    {
        var policy = new StringBuilder();

        Append(policy, "default-src", None);
        Append(policy, "base-uri", None);
        Append(policy, "script-src", None);
        Append(policy, "script-src-attr", None);
        Append(policy, "object-src", None);
        Append(policy, "frame-ancestors", None);
        Append(policy, "form-action", None);

        AppendReport(policy, options.ReportPath);

        return policy.ToString();
    }

    public static string HeaderName(ContentSecurityPolicyOptions options) =>
        options.ReportOnly
            ? "Content-Security-Policy-Report-Only"
            : "Content-Security-Policy";

    private static void AppendReport(StringBuilder policy, string? reportPath)
    {
        if (!string.IsNullOrWhiteSpace(reportPath))
        {
            Append(policy, "report-uri", reportPath);
        }
    }

    private static void Append(StringBuilder policy, string directive, string? value)
    {
        if (policy.Length > 0)
        {
            policy.Append("; ");
        }

        policy.Append(directive);

        if (!string.IsNullOrWhiteSpace(value))
        {
            policy.Append(' ').Append(value);
        }
    }

    private static string Combine(string required, IEnumerable<string>? extra)
    {
        var hosts = Sanitize(extra);
        return hosts.Length == 0 ? required : $"{required} {string.Join(' ', hosts)}";
    }

    private static string ListOrNone(IEnumerable<string>? sources)
    {
        var hosts = Sanitize(sources);
        return hosts.Length == 0 ? None : string.Join(' ', hosts);
    }

    /// <summary>
    /// Drops blank entries and anything containing the header delimiters that would let a
    /// misconfigured value break out of the policy.
    /// </summary>
    private static string[] Sanitize(IEnumerable<string>? sources) =>
        sources?
            .Where(source => !string.IsNullOrWhiteSpace(source))
            .Select(source => source.Trim())
            .Where(source => !source.Contains(';') && !source.Contains(',') && !source.Contains('\n') && !source.Contains('\r'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
        ?? [];
}
