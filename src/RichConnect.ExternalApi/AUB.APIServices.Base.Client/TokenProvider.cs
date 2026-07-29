using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace AUB.APIServices.Base.Client;

public interface ITokenProvider
{
    Task<string> GetAccessToken(CancellationToken cancellationToken);
}

public class TokenProvider(HttpClient httpClient, IDistributedCache cache, IOptions<ServicesConfiguration> options) : ITokenProvider
{
    private const string CacheKey = "AUB.APIServices.AccessToken";
    
    private readonly ServicesConfiguration _configuration = options.Value;
    
    public async Task<string> GetAccessToken(CancellationToken cancellationToken)
    {
        // Try to get from cache
        var cachedTokenBytes = await cache.GetAsync(CacheKey, cancellationToken);
        if (cachedTokenBytes != null && cachedTokenBytes.Length > 0)
        {
            var cachedToken = Encoding.UTF8.GetString(cachedTokenBytes);
            if (!string.IsNullOrEmpty(cachedToken))
            {
                return cachedToken;
            }
        }

        // Fetch new token
        if (!_configuration.NtlmEnabled 
            && !string.IsNullOrEmpty(_configuration.Username) 
            && !string.IsNullOrEmpty(_configuration.Password))
        {
            var credentials = $"{_configuration.Username}:{_configuration.Password}";
            var headerValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", headerValue);
        }
        
        var token = await httpClient.GetFromJsonAsync<AccessToken>(httpClient.BaseAddress, cancellationToken);
        if (token is null) throw new ApplicationException("Failed to get access token");
        
        var accessToken = token.Token;
        
        // Cache the token with sliding expiration
        var cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(20),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_configuration.TokenLifeTime)
        };
        
        var tokenBytes = Encoding.UTF8.GetBytes(accessToken);
        await cache.SetAsync(CacheKey, tokenBytes, cacheOptions, cancellationToken);
        
        return accessToken;
    }
}