using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Events.Notifications;

public sealed class NotificationReadEventHandler : IEventHandler<NotificationReadEvent>
{
    private readonly INotificationCacheService _cache;
    private readonly ILogger<NotificationReadEventHandler> _logger;

    public NotificationReadEventHandler(
        INotificationCacheService cache,
        ILogger<NotificationReadEventHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task HandleAsync(
        NotificationReadEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.DecrementUnreadCountAsync(domainEvent.UserId, cancellationToken);
            _logger.LogInformation(
                "Analytics event {AnalyticsEvent} UserId={UserId} NotificationId={NotificationId}",
                NotificationAnalyticsEvents.Read,
                domainEvent.UserId,
                domainEvent.NotificationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "NotificationRead fan-out failed for {NotificationId}",
                domainEvent.NotificationId);
        }
    }
}
