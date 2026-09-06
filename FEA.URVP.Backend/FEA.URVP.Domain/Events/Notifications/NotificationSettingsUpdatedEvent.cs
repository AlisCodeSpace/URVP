namespace FEA.URVP.Domain.Events.Notifications;

public sealed class NotificationSettingsUpdatedEvent : DomainEvent
{
    public NotificationSettingsUpdatedEvent(
        Guid userId,
        bool emailNotifications,
        bool inAppNotifications)
    {
        UserId = userId;
        EmailNotifications = emailNotifications;
        InAppNotifications = inAppNotifications;
    }

    public Guid UserId { get; }
    public bool EmailNotifications { get; }
    public bool InAppNotifications { get; }
}
