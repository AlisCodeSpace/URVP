namespace FEA.URVP.Domain.Events.Notifications;

public sealed class NotificationDeletedEvent : DomainEvent
{
    public NotificationDeletedEvent(Guid notificationId, Guid userId)
    {
        NotificationId = notificationId;
        UserId = userId;
    }

    public Guid NotificationId { get; }
    public Guid UserId { get; }
}
