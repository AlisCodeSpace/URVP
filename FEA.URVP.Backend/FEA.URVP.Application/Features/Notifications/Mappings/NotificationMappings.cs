using FEA.URVP.Application.DTOs.Notifications;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Mappings;

public static class NotificationMappings
{
    public static NotificationDto ToDto(this Notification notification) => new()
    {
        Id = notification.Id,
        UserId = notification.UserId,
        Type = notification.Type,
        Title = notification.Title,
        Message = notification.Message,
        Data = notification.Data,
        ReferenceId = notification.ReferenceId,
        ReferenceType = notification.ReferenceType,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt,
        ReadAt = notification.ReadAt,
        Priority = notification.Priority,
    };

    public static UserNotificationSettingsDto ToDto(this UserNotificationSettings settings) => new()
    {
        UserId = settings.UserId,
        EmailNotifications = settings.EmailNotifications,
        InAppNotifications = settings.InAppNotifications,
        CreatedAt = settings.CreatedAt,
        UpdatedAt = settings.UpdatedAt,
    };

    public static string ToStorageValue(this NotificationPriority priority) =>
        priority.ToString().ToLowerInvariant();
}
