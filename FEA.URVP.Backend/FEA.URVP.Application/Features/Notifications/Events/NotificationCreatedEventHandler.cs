using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Application.Services.Notifications;
using FEA.URVP.Domain.Events.Notifications;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Events.Notifications;

public sealed class NotificationCreatedEventHandler : IEventHandler<NotificationCreatedEvent>
{
    private readonly NotificationValidationService _validation;
    private readonly INotificationOutboxService _outbox;
    private readonly IPushNotificationService _push;
    private readonly INotificationCacheService _cache;
    private readonly ILogger<NotificationCreatedEventHandler> _logger;

    public NotificationCreatedEventHandler(
        NotificationValidationService validation,
        INotificationOutboxService outbox,
        IPushNotificationService push,
        INotificationCacheService cache,
        ILogger<NotificationCreatedEventHandler> logger)
    {
        _validation = validation;
        _outbox = outbox;
        _push = push;
        _cache = cache;
        _logger = logger;
    }

    public async Task HandleAsync(
        NotificationCreatedEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Track(NotificationAnalyticsEvents.Intended, domainEvent);

            if (await ShouldQueueEmailAsync(domainEvent, cancellationToken))
            {
                await QueueEmailAsync(domainEvent, cancellationToken);
            }

            if (await _validation.ShouldSendPushNotificationAsync(domainEvent.UserId, cancellationToken))
            {
                await QueuePushAsync(domainEvent, cancellationToken);
            }

            await _cache.IncrementUnreadCountAsync(domainEvent.UserId, cancellationToken);
            Track(NotificationAnalyticsEvents.Created, domainEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "NotificationCreated fan-out failed for {NotificationId}; in-app row is unchanged",
                domainEvent.NotificationId);
        }
    }

    private async Task<bool> ShouldQueueEmailAsync(
        NotificationCreatedEvent domainEvent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(domainEvent.Title))
        {
            Track(NotificationAnalyticsEvents.EmailSkipped, domainEvent, "missing title");
            return false;
        }

        if (!await _validation.ShouldSendEmailNotificationAsync(domainEvent.UserId, cancellationToken))
        {
            Track(NotificationAnalyticsEvents.EmailSkipped, domainEvent, "prefs");
            return false;
        }

        if (!await _validation.ValidateNotificationLimitAsync(domainEvent.UserId, cancellationToken))
        {
            Track(NotificationAnalyticsEvents.EmailSkipped, domainEvent, "limit");
            return false;
        }

        return true;
    }

    private async Task QueueEmailAsync(
        NotificationCreatedEvent domainEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            await _outbox.QueueEmailNotificationAsync(domainEvent.NotificationId, cancellationToken);
            Track(NotificationAnalyticsEvents.EmailQueued, domainEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to queue email for notification {NotificationId}",
                domainEvent.NotificationId);
            Track(NotificationAnalyticsEvents.EmailQueueFailed, domainEvent, ex.Message);
        }
    }

    private async Task QueuePushAsync(
        NotificationCreatedEvent domainEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            await _push.QueuePushNotificationAsync(
                domainEvent.UserId,
                domainEvent.Title,
                domainEvent.Message,
                domainEvent.Link,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Push queue failed for notification {NotificationId}",
                domainEvent.NotificationId);
        }
    }

    private void Track(
        string analyticsEvent,
        NotificationCreatedEvent domainEvent,
        string? reason = null) =>
        _logger.LogInformation(
            "Analytics event {AnalyticsEvent} UserId={UserId} NotificationId={NotificationId} Type={Type} Reason={Reason}",
            analyticsEvent,
            domainEvent.UserId,
            domainEvent.NotificationId,
            domainEvent.Type,
            reason);
}
