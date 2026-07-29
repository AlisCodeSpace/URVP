using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace RICHConnect.Backend.Application.Services.Notifications
{
    /// <summary>
    /// Notification cache service implementation using distributed cache (Redis or in-memory fallback)
    /// Production-ready: safe for multi-instance deployments
    /// </summary>
    public class NotificationCacheService : INotificationCacheService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<NotificationCacheService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;
        private readonly TimeSpan _cacheExpiration;
        private const string CacheKeyPrefix = "NotificationCount:";

        public NotificationCacheService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<NotificationCacheService> logger,
            IConfiguration configuration,
            IDistributedCache distributedCache)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            
            // Default cache expiration: 5 minutes
            _cacheExpiration = TimeSpan.FromMinutes(configuration.GetValue<int>("NotificationCache:ExpirationMinutes", 5));
        }

        private static string GetCacheKey(Guid userId) => $"{CacheKeyPrefix}{userId}";

        public async Task<int?> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                var cacheKey = GetCacheKey(userId);
                var cachedBytes = await _distributedCache.GetAsync(cacheKey);

                if (cachedBytes != null && cachedBytes.Length > 0)
                {
                    var cachedValue = Encoding.UTF8.GetString(cachedBytes);
                    if (int.TryParse(cachedValue, out var count))
                    {
                        _logger.LogDebug("Cache hit for user {UserId} unread count: {Count}", userId, count);
                        return count;
                    }
                }

                // Cache miss, get from repository using a scope
                int dbCount;
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                    dbCount = await repository.GetUnreadCountAsync(userId);
                }
                
                await UpdateUnreadCountAsync(userId, dbCount);
                
                _logger.LogDebug("Cache miss for user {UserId}, fetched from DB: {Count}", userId, dbCount);
                return dbCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cached unread count for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> UpdateUnreadCountAsync(Guid userId, int count)
        {
            try
            {
                var cacheKey = GetCacheKey(userId);
                var cachedValue = Encoding.UTF8.GetBytes(count.ToString());
                
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiration
                };

                await _distributedCache.SetAsync(cacheKey, cachedValue, cacheOptions);

                _logger.LogDebug("Updated cached unread count for user {UserId}: {Count}", userId, count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cached unread count for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> InvalidateUnreadCountAsync(Guid userId)
        {
            try
            {
                var cacheKey = GetCacheKey(userId);
                await _distributedCache.RemoveAsync(cacheKey);

                _logger.LogDebug("Invalidated cached unread count for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cached unread count for user {UserId}", userId);
                return false;
            }
        }

        public async Task<int> IncrementUnreadCountAsync(Guid userId)
        {
            try
            {
                // Invalidate cache to force fresh fetch
                await InvalidateUnreadCountAsync(userId);
                
                // Get fresh count from DB using a scope
                int count;
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                    count = await repository.GetUnreadCountAsync(userId);
                }
                
                // Update cache
                await UpdateUnreadCountAsync(userId, count);
                
                _logger.LogDebug("Incremented cached unread count for user {UserId}: {Count}", userId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing cached unread count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<int> DecrementUnreadCountAsync(Guid userId)
        {
            try
            {
                // Invalidate cache to force fresh fetch
                await InvalidateUnreadCountAsync(userId);
                
                // Get fresh count from DB using a scope
                int count;
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                    count = await repository.GetUnreadCountAsync(userId);
                }
                
                // Update cache
                await UpdateUnreadCountAsync(userId, count);
                
                _logger.LogDebug("Decremented cached unread count for user {UserId}: {Count}", userId, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrementing cached unread count for user {UserId}", userId);
                return 0;
            }
        }
    }
}
