using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Interfaces.Notifications
{
    public interface INotificationApplicationService
    {
        Task<Guid> CreateNotificationAsync(CreateNotificationRequest request);
        Task<Notification?> GetNotificationByIdAsync(Guid notificationId, Guid userId);
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page, int pageSize, bool? isRead = null);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task<bool> MarkAllAsReadAsync(Guid userId);
        Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);
        Task<bool> DeleteAllNotificationsAsync(Guid userId);
        Task<UserNotificationSettings?> GetNotificationSettingsAsync(Guid userId);
        Task<bool> UpdateNotificationSettingsAsync(UserNotificationSettings settings);
    }

    public class CreateNotificationRequest
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string? Link { get; set; }
        public string? Priority { get; set; } = "low";
    }
}
