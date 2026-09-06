using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface INotificationOutboxRepository
{
    void Create(NotificationOutbox item);

    Task<IReadOnlyList<NotificationOutbox>> GetPendingItemsAsync(
        int batchSize,
        CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(
        Guid id,
        NotificationOutboxStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);

    Task IncrementRetryAsync(
        Guid id,
        string? errorMessage = null,
        DateTime? nextRetryAt = null,
        CancellationToken cancellationToken = default);

    Task<NotificationOutbox?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<NotificationOutbox?> GetByNotificationIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);
}
