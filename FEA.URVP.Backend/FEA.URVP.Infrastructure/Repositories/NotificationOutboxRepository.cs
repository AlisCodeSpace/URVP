using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class NotificationOutboxRepository : INotificationOutboxRepository
{
    private readonly AppDbContext _db;

    public NotificationOutboxRepository(AppDbContext db)
    {
        _db = db;
    }

    public void Create(NotificationOutbox item) => _db.NotificationOutbox.Add(item);

    public async Task<IReadOnlyList<NotificationOutbox>> GetPendingItemsAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var pending = nameof(NotificationOutboxStatus.Pending);
        var processing = nameof(NotificationOutboxStatus.Processing);

        return await _db.NotificationOutbox
            .Where(x =>
                (x.Status == pending || x.Status == processing)
                && (x.NextRetryAt == null || x.NextRetryAt <= now))
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(
        Guid id,
        NotificationOutboxStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return;
        }

        item.Status = status.ToString();
        if (errorMessage is not null)
        {
            item.ErrorMessage = Truncate(errorMessage, NotificationOutbox.ErrorMessageMaxLength);
        }

        if (status is NotificationOutboxStatus.Completed or NotificationOutboxStatus.Failed)
        {
            item.ProcessedAt = DateTime.UtcNow;
        }
    }

    public async Task IncrementRetryAsync(
        Guid id,
        string? errorMessage = null,
        DateTime? nextRetryAt = null,
        CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return;
        }

        item.RetryCount++;
        item.NextRetryAt = nextRetryAt;
        if (errorMessage is not null)
        {
            item.ErrorMessage = Truncate(errorMessage, NotificationOutbox.ErrorMessageMaxLength);
        }
    }

    public Task<NotificationOutbox?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.NotificationOutbox.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<NotificationOutbox?> GetByNotificationIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default) =>
        _db.NotificationOutbox.FirstOrDefaultAsync(
            x => x.NotificationId == notificationId,
            cancellationToken);

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
