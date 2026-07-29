namespace RICHConnect.Backend.Application.Services.Security
{
    /// <summary>
    /// Manages secure access to application secrets
    /// </summary>
    public class SecretManager
    {
        private readonly IConfiguration _configuration;
        private readonly bool _isDesignTime;

        public SecretManager(IConfiguration configuration)
        {
            _configuration = configuration;
            // Check if we're running in design-time (e.g., migrations)
            _isDesignTime = AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.FullName?.Contains("Microsoft.EntityFrameworkCore.Design") == true);
        }

        /// <summary>
        /// Gets Azure B2C client secret from environment variables or falls back to configuration
        /// </summary>
        public string? GetAzureB2CClientSecret()
        {
            return Environment.GetEnvironmentVariable("AZURE_B2C_CLIENT_SECRET")
                ?? _configuration["AzureB2C:ClientSecret"];
        }
    }
} 