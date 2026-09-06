namespace FEA.URVP.Application.Abstractions.Notifications;

public interface INotificationOutboxService
{
    Task QueueEmailNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);

    Task ProcessOutboxAsync(CancellationToken cancellationToken = default);
}
