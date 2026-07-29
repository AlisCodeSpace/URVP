using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using ProtoBuf.Grpc.ClientFactory;
using Serilog;

namespace AUB.APIServices.Base.Client;

public static class BuilderExtension
{
    public static ServicesConfiguration AddApiServices(this IHostApplicationBuilder builder)
    {
        var validateSsl = builder.Environment.IsProduction();
        var configuration = builder.Configuration;
        return AddApiServicesInternal(builder.Services, configuration, validateSsl);
    }
    public static ServicesConfiguration AddApiServicesInternal(this IServiceCollection services, IConfiguration configuration, bool validateSsl = true)
    {
        // Note: Distributed cache (Redis) should be registered before this via AddDistributedCache()
        // This ensures IDistributedCache is available for TokenProvider
        
        //Builder gRPC Services Configuration
        var servicesConfigurationEndPoint = configuration["ServicesConfigurationEndPoint"]!;
        var servicesConfiguration = FetchConfigFromServicesEnPoint(servicesConfigurationEndPoint, validateSsl);
        var ntlmEnabled = CanUseNtlm();
        servicesConfiguration.NtlmEnabled = ntlmEnabled;
        servicesConfiguration.Username = ntlmEnabled ? string.Empty : configuration["Application:Username"]!;
        servicesConfiguration.Password = ntlmEnabled ? string.Empty : configuration["Application:Password"]!;
        servicesConfiguration.ValidateSsl = validateSsl;
        
        services.Configure<ServicesConfiguration>(options =>
        {
            options.Basic = servicesConfiguration.Basic;
            options.Integrated = servicesConfiguration.Integrated;
            options.EndPoints = servicesConfiguration.EndPoints;
            options.PublicKeys = servicesConfiguration.PublicKeys;
            options.NtlmEnabled = servicesConfiguration.NtlmEnabled;
            options.Username = servicesConfiguration.Username;
            options.Password = servicesConfiguration.Password;
            options.ValidateSsl = servicesConfiguration.ValidateSsl;
            options.TokenLifeTime = servicesConfiguration.TokenLifeTime;
            options.SignatureKeyLifeTime = servicesConfiguration.SignatureKeyLifeTime;
        });

        services.AddHttpClient<ITokenProvider, TokenProvider>((client) =>
        {
            var authUrl = servicesConfiguration.NtlmEnabled
                ? servicesConfiguration.Integrated
                : servicesConfiguration.Basic;
            client.BaseAddress = new Uri(authUrl);
        }).ConfigurePrimaryHttpMessageHandler(() =>
        {
            var socketsHttpHandler = new SocketsHttpHandler()
            {
                UseCookies = false,
                AllowAutoRedirect = false,
                
                //set pooled connection lifetime to 2 hours. this is for any change in DNS
                PooledConnectionLifetime = TimeSpan.FromMinutes(120)
            };
            
            //don't use ntlm in non-windows environment
            if (servicesConfiguration.NtlmEnabled) socketsHttpHandler.Credentials = CredentialCache.DefaultNetworkCredentials;
            
            //return if in production else ignore ssl certificate verification
            if (validateSsl) return socketsHttpHandler;
            
            socketsHttpHandler.SslOptions = new SslClientAuthenticationOptions()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
            };
            
            return socketsHttpHandler;
        }).SetHandlerLifetime(Timeout.InfiniteTimeSpan);
        
        return servicesConfiguration;
    }

    public static void AddAubGrpcClient<T>(this IServiceCollection services, string serviceName, ServicesConfiguration config)
        where T : class
    {
        var serviceEndPoint = config.EndPoints != null
                          && config.EndPoints.TryGetValue(serviceName, out var endPoint)
            ? new Uri(endPoint)
            : null;
        if (serviceEndPoint == null) throw new ApplicationException($"No {serviceName} configured.");
        
        services.AddCodeFirstGrpcClient<T>(options => { options.Address = serviceEndPoint; })
            .AddCallCredentials(async (context, metadata, serviceProvider) =>
            {
                var provider = serviceProvider.GetRequiredService<ITokenProvider>();
                var token = await provider.GetAccessToken(context.CancellationToken);
                metadata.Add("Authorization", $"Bearer {token}");
            }).ConfigurePrimaryHttpMessageHandler(() => config.ValidateSsl
                ? new HttpClientHandler()
                : new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
    }

    private static ServicesConfiguration FetchConfigFromServicesEnPoint(string servicesEndPoint, bool validateSsl, int maxRetries = 2)
    {
        const int baseDelaySeconds = 1;
        const int timeoutSeconds = 30;
        
        // SSRF Protection: Validate endpoint URL before making requests
        if (!Uri.TryCreate(servicesEndPoint, UriKind.Absolute, out var endpointUri))
        {
            throw new InvalidOperationException(
                $"ServicesConfigurationEndPoint must be a valid absolute URL. Provided: {servicesEndPoint}");
        }

        // SSRF Protection: Only allow HTTPS endpoints
        if (endpointUri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                $"ServicesConfigurationEndPoint must use HTTPS scheme. Provided: {endpointUri.Scheme}");
        }

        // SSRF Protection: Block loopback/localhost addresses
        if (endpointUri.IsLoopback || 
            endpointUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("192.168.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("10.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.16.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.17.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.18.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.19.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.20.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.21.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.22.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.23.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.24.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.25.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.26.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.27.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.28.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.29.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.30.", StringComparison.OrdinalIgnoreCase) ||
            endpointUri.Host.StartsWith("172.31.", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"ServicesConfigurationEndPoint must not target loopback or private network addresses. Provided: {endpointUri.Host}");
        }

        Exception? lastException = null;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                Log.Information("Attempting to fetch FMIS configuration from {Endpoint} (Attempt {Attempt}/{MaxRetries})", 
                    servicesEndPoint, attempt, maxRetries);
                
                using var handler = new HttpClientHandler();
                if (!validateSsl) handler.ServerCertificateCustomValidationCallback = delegate { return true; };
                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(timeoutSeconds)
                };
                
                var json = client.GetStringAsync(servicesEndPoint).Result;
                var configuration = System.Text.Json.JsonSerializer.Deserialize<ServicesConfiguration>(json)!;
                
                Log.Information("Successfully fetched FMIS configuration from {Endpoint} on attempt {Attempt}", 
                    servicesEndPoint, attempt);
                
                return configuration;
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                if (attempt < maxRetries)
                {
                    var delaySeconds = baseDelaySeconds * (int)Math.Pow(2, attempt - 1); // Exponential backoff: 1s, 2s, 4s, 8s, 16s (with maxRetries=2, only 1s delay is used)
                    Log.Warning(ex, 
                        "Failed to fetch FMIS configuration from {Endpoint} on attempt {Attempt}/{MaxRetries}. " +
                        "Retrying in {DelaySeconds} seconds... Error: {ErrorMessage}", 
                        servicesEndPoint, attempt, maxRetries, delaySeconds, ex.Message);
                    
                    Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                }
                else
                {
                    Log.Error(ex, 
                        "Failed to fetch FMIS configuration from {Endpoint} after {MaxRetries} attempts. " +
                        "All retry attempts exhausted.", 
                        servicesEndPoint, maxRetries);
                }
            }
        }
        
        // If we get here, all retries failed
        throw new InvalidOperationException(
            $"Failed to fetch FMIS configuration from {servicesEndPoint} after {maxRetries} attempts. " +
            "The service may be unreachable or the connection is too slow.", 
            lastException);
    }
    private static bool CanUseNtlm()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return false;
        try
        {
            _ = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain();
        }
        catch
        {
            return false;
        }
        return true;
    }
}