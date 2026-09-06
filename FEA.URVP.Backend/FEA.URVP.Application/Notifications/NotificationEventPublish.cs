using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Domain.Events;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Notifications;

internal static class NotificationEventPublish
{
    public static async Task TryPublishAsync<TEvent>(
        IEventBus eventBus,
        TEvent domainEvent,
        ILogger logger,
        CancellationToken cancellationToken)
        where TEvent : IDomainEvent
    {
        try
        {
            await eventBus.PublishAsync(domainEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to publish {EventType} ({EventId}) after commit",
                domainEvent.EventType,
                domainEvent.EventId);
        }
    }
}
