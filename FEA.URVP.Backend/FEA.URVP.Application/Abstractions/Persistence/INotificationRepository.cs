using FEA.URVP.Domain.Entities.Notifications;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetUserNotificationsAsync(
        Guid userId,
        int page,
        int pageSize,
        bool? isRead,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<int> GetTotalCountAsync(Guid userId, CancellationToken cancellationToken = default);

    void Create(Notification notification);

    void Update(Notification notification);

    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<int> DeleteAllAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Notification?> FindByReferenceAsync(
        Guid userId,
        string type,
        Guid referenceId,
        CancellationToken cancellationToken = default);

    Task<UserNotificationSettings?> GetSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    void CreateSettings(UserNotificationSettings settings);

    void UpdateSettings(UserNotificationSettings settings);
}
