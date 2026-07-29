using RICHConnect.Backend.Domain.Enums;
namespace RICHConnect.Backend.Domain.Events
{
    public class NotificationCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "NotificationCreated";
        
        public Guid NotificationId { get; }
        public Guid UserId { get; }
        public string Title { get; }
        public string Message { get; }
        public NotificationType Type { get; }
        public string? Link { get; }
        public string Priority { get; }
        
        public NotificationCreatedEvent(
            Guid notificationId,
            Guid userId,
            string title,
            string message,
            NotificationType type,
            string? link = null,
            string priority = "low")
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            NotificationId = notificationId;
            UserId = userId;
            Title = title;
            Message = message;
            Type = type;
            Link = link;
            Priority = priority;
        }
    }

    public class NotificationReadEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "NotificationRead";
        
        public Guid NotificationId { get; }
        public Guid UserId { get; }
        public DateTime ReadAt { get; }
        
        public NotificationReadEvent(Guid notificationId, Guid userId, DateTime readAt)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            NotificationId = notificationId;
            UserId = userId;
            ReadAt = readAt;
        }
    }

    public class NotificationDeletedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "NotificationDeleted";
        
        public Guid NotificationId { get; }
        public Guid UserId { get; }
        
        public NotificationDeletedEvent(Guid notificationId, Guid userId)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            NotificationId = notificationId;
            UserId = userId;
        }
    }

    public class NotificationSettingsUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "NotificationSettingsUpdated";
        
        public Guid UserId { get; }
        public bool EmailNotifications { get; }
        public bool InAppNotifications { get; }
        
        public NotificationSettingsUpdatedEvent(
            Guid userId,
            bool emailNotifications,
            bool inAppNotifications)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            EmailNotifications = emailNotifications;
            InAppNotifications = inAppNotifications;
        }
    }
}

