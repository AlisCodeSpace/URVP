using System.Text;
using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Infrastructure.Notifications;

public sealed class NotificationCacheService : INotificationCacheService
{
    private readonly IDistributedCache _cache;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NotificationCacheOptions _options;
    private readonly ILogger<NotificationCacheService> _logger;

    public NotificationCacheService(
        IDistributedCache cache,
        IServiceScopeFactory scopeFactory,
        IOptions<NotificationCacheOptions> options,
        ILogger<NotificationCacheService> logger)
    {
        _cache = cache;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<int?> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var raw = await _cache.GetAsync(Key(userId), cancellationToken);
            if (raw is null || raw.Length == 0)
            {
                return null;
            }

            return int.TryParse(Encoding.UTF8.GetString(raw), out var count) ? count : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read unread cache for {UserId}", userId);
            return null;
        }
    }

    public async Task UpdateUnreadCountAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var minutes = _options.ExpirationMinutes > 0 ? _options.ExpirationMinutes : 5;
            await _cache.SetAsync(
                Key(userId),
                Encoding.UTF8.GetBytes(count.ToString()),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes),
                },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update unread cache for {UserId}", userId);
        }
    }

    public async Task InvalidateUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(Key(userId), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invalidate unread cache for {UserId}", userId);
        }
    }

    public Task IncrementUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default) =>
        RefreshFromDatabaseAsync(userId, cancellationToken);

    public Task DecrementUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default) =>
        RefreshFromDatabaseAsync(userId, cancellationToken);

    private async Task RefreshFromDatabaseAsync(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            await InvalidateUnreadCountAsync(userId, cancellationToken);

            using var scope = _scopeFactory.CreateScope();
            var notifications = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var count = await notifications.GetUnreadCountAsync(userId, cancellationToken);
            await UpdateUnreadCountAsync(userId, count, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh unread cache for {UserId}", userId);
        }
    }

    private static string Key(Guid userId) => $"NotificationCount:{userId}";
}
