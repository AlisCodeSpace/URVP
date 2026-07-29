using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RICHConnect.Backend.Api.HealthChecks
{
    /// <summary>
    /// Health check for FMIS external service availability
    /// </summary>
    public class FmisHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FmisHealthCheck> _logger;
        private readonly IHttpClientFactory? _httpClientFactory;

        public FmisHealthCheck(
            IConfiguration configuration,
            ILogger<FmisHealthCheck> logger,
            IHttpClientFactory? httpClientFactory = null)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var fmisEndpoint = _configuration["ServicesConfigurationEndPoint"];
                
                if (string.IsNullOrEmpty(fmisEndpoint))
                {
                    return HealthCheckResult.Degraded(
                        "FMIS endpoint not configured",
                        data: new Dictionary<string, object>
                        {
                            ["configured"] = false
                        });
                }

                // If HttpClientFactory is available, do a quick connectivity check
                if (_httpClientFactory != null)
                {
                    using var httpClient = _httpClientFactory.CreateClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(3);

                    try
                    {
                        // Just check if the endpoint is reachable (don't need actual auth)
                        var response = await httpClient.GetAsync(
                            fmisEndpoint,
                            HttpCompletionOption.ResponseHeadersRead,
                            cancellationToken);

                        // Any response (even 401/403) means the service is up
                        return HealthCheckResult.Healthy(
                            "FMIS service is reachable",
                            data: new Dictionary<string, object>
                            {
                                ["endpoint"] = fmisEndpoint,
                                ["statusCode"] = (int)response.StatusCode
                            });
                    }
                    catch (TaskCanceledException)
                    {
                        return HealthCheckResult.Degraded(
                            "FMIS service timeout",
                            data: new Dictionary<string, object>
                            {
                                ["endpoint"] = fmisEndpoint,
                                ["error"] = "timeout"
                            });
                    }
                    catch (HttpRequestException ex)
                    {
                        return HealthCheckResult.Degraded(
                            $"FMIS service unreachable: {ex.Message}",
                            data: new Dictionary<string, object>
                            {
                                ["endpoint"] = fmisEndpoint,
                                ["error"] = ex.Message
                            });
                    }
                }

                // If no HttpClientFactory, just report that FMIS is configured
                return HealthCheckResult.Healthy(
                    "FMIS endpoint configured",
                    data: new Dictionary<string, object>
                    {
                        ["endpoint"] = fmisEndpoint,
                        ["note"] = "connectivity not checked"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking FMIS health");
                return HealthCheckResult.Degraded(
                    $"FMIS health check error: {ex.Message}",
                    exception: ex);
            }
        }
    }
}
