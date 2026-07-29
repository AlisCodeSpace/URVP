using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id);
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page, int pageSize, bool? isRead = null);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<int> GetTotalCountAsync(Guid userId, bool? isRead = null);
        Task<Notification?> FindByReferenceAsync(Guid userId, NotificationType type, Guid referenceId);
        Task<Notification> CreateAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task MarkAsReadAsync(Guid notificationId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
        Task<UserNotificationSettings?> GetUserSettingsAsync(Guid userId);
        Task UpdateUserSettingsAsync(UserNotificationSettings settings);
        Task<UserNotificationSettings?> GetUserNotificationSettingsAsync(Guid userId);
        Task CreateUserNotificationSettingsAsync(UserNotificationSettings settings);
    }
}
