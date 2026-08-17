using FEA.URVP.Api.Configuration.Security;

namespace FEA.URVP.Api.Services;

/// <summary>
/// Validates return URLs against configured CORS origins to prevent open redirects.
/// </summary>
public sealed class ReturnUrlValidationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReturnUrlValidationService> _logger;
    private readonly IWebHostEnvironment _environment;

    public ReturnUrlValidationService(
        IConfiguration configuration,
        ILogger<ReturnUrlValidationService> logger,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _logger = logger;
        _environment = environment;
    }

    public string ValidateReturnUrl(string? returnUrl)
    {
        var allowedOrigins = CorsOrigins.GetAllowedOrigins(_configuration);
        var defaultReturnUrl = allowedOrigins.FirstOrDefault()
            ?? (_environment.IsDevelopment() ? "https://localhost:3000" : "/");

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return defaultReturnUrl;
        }

        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
        {
            _logger.LogWarning("Invalid returnUrl format: {ReturnUrl}. Using default.", returnUrl);
            return defaultReturnUrl;
        }

        var origin = $"{uri.Scheme}://{uri.Authority}";
        if (allowedOrigins.Any(allowed => allowed.Equals(origin, StringComparison.OrdinalIgnoreCase)))
        {
            return returnUrl;
        }

        _logger.LogWarning("ReturnUrl origin not in allowlist: {Origin}. Using default.", origin);
        return defaultReturnUrl;
    }
}
