using FEA.URVP.Application.DTOs.Notifications;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Abstractions.Notifications;

public interface INotificationApplicationService
{
    Task<Guid?> CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        string? link = null,
        NotificationPriority? priority = null,
        Guid? referenceId = null,
        string? referenceType = null,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<NotificationDto> Items, int TotalCount)> GetUserNotificationsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        bool? isRead = null,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<NotificationDto> GetByIdAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserNotificationSettingsDto?> GetSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserNotificationSettingsDto> UpdateSettingsAsync(
        Guid userId,
        bool emailNotifications,
        bool inAppNotifications,
        CancellationToken cancellationToken = default);
}
