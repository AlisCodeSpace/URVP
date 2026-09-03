using FEA.URVP.Api.Configuration.Security;

namespace FEA.URVP.Api.Services;

/// <summary>
/// Validates post-login and post-logout return URLs against an explicit same-origin allow-list,
/// so a crafted <c>returnUrl</c> cannot turn the SSO endpoints into an open redirect.
/// </summary>
/// <remarks>
/// Two shapes are accepted. A root-relative path is same-origin by construction and is the normal
/// case for the same-origin BFF deployment. An absolute URL must match either the origin serving
/// the current request or an entry in <c>Cors:AllowedOrigins</c>, which is what the local
/// <c>next dev</c> split-port topology relies on.
/// </remarks>
public sealed class ReturnUrlValidationService
{
    private const string DefaultRelativeReturnUrl = "/";

    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ReturnUrlValidationService> _logger;

    public ReturnUrlValidationService(
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment environment,
        ILogger<ReturnUrlValidationService> logger)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _environment = environment;
        _logger = logger;
    }

    public string ValidateReturnUrl(string? returnUrl)
    {
        var fallback = DefaultReturnUrl();

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return fallback;
        }

        var candidate = returnUrl.Trim();

        if (IsSafeRelativePath(candidate))
        {
            return candidate;
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            // Logged without the value: an attacker-supplied URL in the log is a log-injection
            // and phishing vector for whoever reads it later.
            _logger.LogWarning("Rejected returnUrl: not a root-relative path or absolute URL. Using default.");
            return fallback;
        }

        var origin = $"{uri.Scheme}://{uri.Authority}";

        if (AllowedOrigins().Any(allowed => allowed.Equals(origin, StringComparison.OrdinalIgnoreCase)))
        {
            return candidate;
        }

        _logger.LogWarning("Rejected returnUrl with non-allow-listed origin {Origin}. Using default.", origin);
        return fallback;
    }

    /// <summary>
    /// Builds a URL for a frontend path. Same-origin deployments keep it relative; the
    /// split-origin development topology prefixes the allow-listed frontend origin.
    /// </summary>
    public string BuildFrontendUrl(string relativePath)
    {
        var path = relativePath.StartsWith('/') ? relativePath : "/" + relativePath;

        if (!_environment.IsDevelopment())
        {
            return path;
        }

        var origin = CorsOrigins.GetAllowedOrigins(_configuration, allowInsecureLoopback: true).FirstOrDefault();
        return origin is null ? path : origin + path;
    }

    /// <summary>
    /// Accepts <c>/path</c> but not <c>//host</c> (protocol-relative, resolves off-origin),
    /// <c>/\host</c> (treated as protocol-relative by some browsers), or anything carrying
    /// control characters that could split a Location header.
    /// </summary>
    private static bool IsSafeRelativePath(string value)
    {
        if (value.Length == 0 || value[0] != '/')
        {
            return false;
        }

        if (value.Length > 1 && (value[1] == '/' || value[1] == '\\'))
        {
            return false;
        }

        return !value.Any(character => char.IsControl(character));
    }

    private IEnumerable<string> AllowedOrigins()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is not null && request.Host.HasValue)
        {
            yield return $"{request.Scheme}://{request.Host.Value}";
        }

        foreach (var origin in CorsOrigins.GetAllowedOrigins(_configuration, _environment.IsDevelopment()))
        {
            yield return origin;
        }
    }

    /// <summary>
    /// Same-origin deployments return to the application root. The split-origin development
    /// topology has no same-origin frontend, so the first allow-listed origin is used instead.
    /// </summary>
    private string DefaultReturnUrl()
    {
        if (!_environment.IsDevelopment())
        {
            return DefaultRelativeReturnUrl;
        }

        return CorsOrigins.GetAllowedOrigins(_configuration, allowInsecureLoopback: true).FirstOrDefault()
            ?? DefaultRelativeReturnUrl;
    }
}
