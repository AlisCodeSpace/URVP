using FEA.URVP.Api.Configuration.Security;

namespace FEA.URVP.Tests.Security;

public sealed class ContentSecurityPolicyTests
{
    private static ContentSecurityPolicyOptions Options() => new();

    [Fact]
    public void Document_policy_uses_a_nonce_and_never_unsafe_inline_script()
    {
        var policy = ContentSecurityPolicy.BuildForDocument(Options(), "n0nc3value");

        Assert.Contains("script-src 'self' 'nonce-n0nc3value'", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("script-src 'self' 'unsafe-inline'", policy, StringComparison.Ordinal);
        Assert.Contains("script-src-attr 'none'", policy, StringComparison.Ordinal);
    }

    [Fact]
    public void Document_policy_locks_down_the_baseline_directives()
    {
        var policy = ContentSecurityPolicy.BuildForDocument(Options(), "nonce");

        Assert.Contains("default-src 'self'", policy, StringComparison.Ordinal);
        Assert.Contains("base-uri 'self'", policy, StringComparison.Ordinal);
        Assert.Contains("object-src 'none'", policy, StringComparison.Ordinal);
        Assert.Contains("frame-ancestors 'none'", policy, StringComparison.Ordinal);
        Assert.Contains("upgrade-insecure-requests", policy, StringComparison.Ordinal);
    }

    [Fact]
    public void Api_policy_is_stricter_than_the_document_policy()
    {
        var policy = ContentSecurityPolicy.BuildForApi(Options());

        Assert.Contains("default-src 'none'", policy, StringComparison.Ordinal);
        Assert.Contains("script-src 'none'", policy, StringComparison.Ordinal);
        Assert.Contains("frame-ancestors 'none'", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("'self'", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("nonce", policy, StringComparison.Ordinal);
    }

    [Fact]
    public void Configured_hosts_extend_rather_than_replace_self()
    {
        var options = Options();
        options.ConnectSrc.Add("https://login.microsoftonline.com");
        options.FontSrc.Add("https://fonts.gstatic.com");

        var policy = ContentSecurityPolicy.BuildForDocument(options, "nonce");

        Assert.Contains(
            "connect-src 'self' https://login.microsoftonline.com",
            policy,
            StringComparison.Ordinal);
        Assert.Contains("font-src 'self' https://fonts.gstatic.com", policy, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("https://evil.example; script-src 'unsafe-inline'")]
    [InlineData("https://evil.example, https://other.example")]
    [InlineData("https://evil.example\r\nX-Injected: 1")]
    public void Malformed_configured_hosts_cannot_inject_extra_directives(string host)
    {
        var options = Options();
        options.ConnectSrc.Add(host);

        var policy = ContentSecurityPolicy.BuildForDocument(options, "nonce");

        Assert.Contains("connect-src 'self';", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("evil.example", policy, StringComparison.Ordinal);
        Assert.DoesNotContain("X-Injected", policy, StringComparison.Ordinal);
    }

    [Fact]
    public void Empty_frame_lists_become_none_rather_than_being_omitted()
    {
        var policy = ContentSecurityPolicy.BuildForDocument(Options(), "nonce");

        Assert.Contains("frame-src 'none'", policy, StringComparison.Ordinal);
        Assert.Contains("frame-ancestors 'none'", policy, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_path_is_appended_when_configured()
    {
        var policy = ContentSecurityPolicy.BuildForDocument(Options(), "nonce");

        Assert.Contains("report-uri /api/security/csp-report", policy, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_only_switches_the_header_name()
    {
        var enforcing = Options();
        var reportOnly = new ContentSecurityPolicyOptions { ReportOnly = true };

        Assert.Equal("Content-Security-Policy", ContentSecurityPolicy.HeaderName(enforcing));
        Assert.Equal(
            "Content-Security-Policy-Report-Only",
            ContentSecurityPolicy.HeaderName(reportOnly));
    }
}
