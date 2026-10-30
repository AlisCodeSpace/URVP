using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Events.Notifications;

public sealed class NotificationDeletedEventHandler : IEventHandler<NotificationDeletedEvent>
{
    private readonly INotificationCacheService _cache;
    private readonly ILogger<NotificationDeletedEventHandler> _logger;

    public NotificationDeletedEventHandler(
        INotificationCacheService cache,
        ILogger<NotificationDeletedEventHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task HandleAsync(
        NotificationDeletedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.InvalidateUnreadCountAsync(domainEvent.UserId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "NotificationDeleted fan-out failed for {NotificationId}",
                domainEvent.NotificationId);
        }
    }
}
