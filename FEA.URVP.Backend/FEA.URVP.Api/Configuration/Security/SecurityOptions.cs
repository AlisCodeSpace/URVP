namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// Production security knobs bound from the <c>Security</c> configuration section, so a
/// deployment can tighten CSP hosts, HSTS, trusted proxies and rate limits without a rebuild.
/// </summary>
public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public FrontendHostingOptions Frontend { get; set; } = new();
    public ContentSecurityPolicyOptions ContentSecurityPolicy { get; set; } = new();
    public HstsOptions Hsts { get; set; } = new();
    public HttpsOptions Https { get; set; } = new();
    public TrustedProxyOptions TrustedProxies { get; set; } = new();
    public RateLimitingOptions RateLimiting { get; set; } = new();
    public HealthOptions Health { get; set; } = new();
}

public sealed class HttpsOptions
{
    /// <summary>
    /// Leave null to keep the platform-aware default: redirect everywhere except on a PaaS host
    /// that terminates TLS and exposes only a plain-HTTP port to the container, where redirecting
    /// produces a loop.
    /// </summary>
    public bool? RedirectToHttps { get; set; }
}

/// <summary>
/// Hosting of the statically exported Next.js app from this process (same-origin BFF).
/// </summary>
public sealed class FrontendHostingOptions
{
    /// <summary>
    /// Directory holding the exported frontend. Relative paths resolve against the content root.
    /// </summary>
    public string RootPath { get; set; } = "wwwroot";

    /// <summary>
    /// Set false to run the backend as an API-only host (local <c>next dev</c> topology).
    /// </summary>
    public bool Enabled { get; set; } = true;
}

public sealed class ContentSecurityPolicyOptions
{
    /// <summary>
    /// Emit <c>Content-Security-Policy-Report-Only</c> instead of the enforcing header.
    /// Intended for staging rollout only.
    /// </summary>
    public bool ReportOnly { get; set; }

    public string ReportPath { get; set; } = "/api/security/csp-report";

    /// <summary>Extra origins the browser may call with fetch/XHR beyond <c>'self'</c>.</summary>
    public IList<string> ConnectSrc { get; set; } = [];

    /// <summary>Extra image origins beyond <c>'self'</c> and <c>data:</c>.</summary>
    public IList<string> ImgSrc { get; set; } = [];

    /// <summary>Extra font origins beyond <c>'self'</c>.</summary>
    public IList<string> FontSrc { get; set; } = [];

    /// <summary>Frame sources. Empty means <c>'none'</c>.</summary>
    public IList<string> FrameSrc { get; set; } = [];

    /// <summary>Who may frame this app. Empty means <c>'none'</c>.</summary>
    public IList<string> FrameAncestors { get; set; } = [];

    /// <summary>Extra form targets beyond <c>'self'</c>.</summary>
    public IList<string> FormAction { get; set; } = [];
}

public sealed class HstsOptions
{
    public int MaxAgeDays { get; set; } = 365;

    /// <summary>Only enable once every subdomain of the production host serves HTTPS.</summary>
    public bool IncludeSubDomains { get; set; }

    /// <summary>Only enable after confirming the hstspreload.org submission requirements.</summary>
    public bool Preload { get; set; }
}

public sealed class TrustedProxyOptions
{
    /// <summary>Explicit reverse-proxy IP addresses permitted to set X-Forwarded-*.</summary>
    public IList<string> KnownProxies { get; set; } = [];

    /// <summary>Explicit reverse-proxy CIDR networks permitted to set X-Forwarded-*.</summary>
    public IList<string> KnownNetworks { get; set; } = [];

    public int ForwardLimit { get; set; } = 1;

    /// <summary>
    /// Escape hatch for PaaS hosts whose proxy addresses are not stable (for example Render).
    /// Must stay false behind IIS or any proxy with a known address, otherwise clients can
    /// spoof their own scheme and IP.
    /// </summary>
    public bool TrustAnyProxy { get; set; }
}

/// <summary>
/// Fixed-window request budgets. Counters are per-instance; see <c>docs/SECURITY.md</c> for the
/// single-instance deployment constraint this assumes.
/// </summary>
public sealed class RateLimitingOptions
{
    public int GlobalPermitPerMinute { get; set; } = 300;
    public int AuthPermitPerMinute { get; set; } = 20;
    public int UploadPermitPerMinute { get; set; } = 30;
    public int DownloadPermitPerMinute { get; set; } = 120;
    public int ReportPermitPerMinute { get; set; } = 10;
    public int QueueLimit { get; set; } = 0;
}

public sealed class HealthOptions
{
    /// <summary>
    /// CIDR networks or bare IPs allowed to read detailed readiness output without a session.
    /// Empty means detailed readiness requires an authenticated administrator.
    /// </summary>
    public IList<string> MonitoringNetworks { get; set; } = [];
}
