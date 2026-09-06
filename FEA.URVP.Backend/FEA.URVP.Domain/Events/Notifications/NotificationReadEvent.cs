namespace FEA.URVP.Domain.Events.Notifications;

public sealed class NotificationReadEvent : DomainEvent
{
    public NotificationReadEvent(Guid notificationId, Guid userId, DateTime readAt)
    {
        NotificationId = notificationId;
        UserId = userId;
        ReadAt = readAt;
    }

    public Guid NotificationId { get; }
    public Guid UserId { get; }
    public DateTime ReadAt { get; }
}
