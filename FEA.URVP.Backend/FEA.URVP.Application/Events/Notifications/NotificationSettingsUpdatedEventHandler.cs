using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Events.Notifications;

public sealed class NotificationSettingsUpdatedEventHandler
    : IEventHandler<NotificationSettingsUpdatedEvent>
{
    private readonly INotificationCacheService _cache;
    private readonly ILogger<NotificationSettingsUpdatedEventHandler> _logger;

    public NotificationSettingsUpdatedEventHandler(
        INotificationCacheService cache,
        ILogger<NotificationSettingsUpdatedEventHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task HandleAsync(
        NotificationSettingsUpdatedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.InvalidateUnreadCountAsync(domainEvent.UserId, cancellationToken);
            _logger.LogInformation(
                "Notification settings updated for {UserId}: email={EmailNotifications} inApp={InAppNotifications}",
                domainEvent.UserId,
                domainEvent.EmailNotifications,
                domainEvent.InAppNotifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "NotificationSettingsUpdated fan-out failed for {UserId}",
                domainEvent.UserId);
        }
    }
}
