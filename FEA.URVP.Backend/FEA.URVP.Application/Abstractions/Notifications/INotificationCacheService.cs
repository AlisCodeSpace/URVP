namespace FEA.URVP.Application.Abstractions.Notifications;

public interface INotificationCacheService
{
    Task<int?> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task UpdateUnreadCountAsync(Guid userId, int count, CancellationToken cancellationToken = default);

    Task InvalidateUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task IncrementUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task DecrementUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
