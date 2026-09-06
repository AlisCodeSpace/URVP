namespace FEA.URVP.Domain.Events.Notifications;

public sealed class NotificationCreatedEvent : DomainEvent
{
    public NotificationCreatedEvent(
        Guid notificationId,
        Guid userId,
        string title,
        string message,
        string type,
        string? link,
        string priority)
    {
        NotificationId = notificationId;
        UserId = userId;
        Title = title;
        Message = message;
        Type = type;
        Link = link;
        Priority = priority;
    }

    public Guid NotificationId { get; }
    public Guid UserId { get; }
    public string Title { get; }
    public string Message { get; }
    public string Type { get; }
    public string? Link { get; }
    public string Priority { get; }
}
