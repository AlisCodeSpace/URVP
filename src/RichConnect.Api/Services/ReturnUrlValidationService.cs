using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RICHConnect.Backend.Api.Services
{
    /// <summary>
    /// Service for validating return URLs against configured allowed origins to prevent open redirects
    /// </summary>
    public class ReturnUrlValidationService
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

        /// <summary>
        /// Validates returnUrl against configured allowed origins to prevent open redirects
        /// </summary>
        public string ValidateReturnUrl(string? returnUrl)
        {
            // Get allowed origins from CORS configuration
            var allowedOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            
            // Default fallback (only use localhost in development)
            var defaultReturnUrl = allowedOrigins.FirstOrDefault() 
                ?? (_environment.IsDevelopment() ? "https://localhost:3000" : "/");

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return defaultReturnUrl;
            }

            // Parse the returnUrl
            if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri))
            {
                _logger.LogWarning("Invalid returnUrl format: {ReturnUrl}. Using default.", returnUrl);
                return defaultReturnUrl;
            }

            // Check if origin is in allowlist
            var origin = $"{uri.Scheme}://{uri.Authority}";
            if (allowedOrigins.Any(allowed => allowed.Equals(origin, StringComparison.OrdinalIgnoreCase)))
            {
                return returnUrl;
            }

            _logger.LogWarning("ReturnUrl origin not in allowlist: {Origin}. Using default.", origin);
            return defaultReturnUrl;
        }
    }
}
