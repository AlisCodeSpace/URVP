using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Notifications;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Notifications.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetUserNotificationsAsync(
        Guid userId,
        int page,
        int pageSize,
        bool? isRead,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Notifications.AsNoTracking().Where(x => x.UserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(x => x.IsRead == isRead.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);

    public Task<int> GetTotalCountAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.Notifications.CountAsync(x => x.UserId == userId, cancellationToken);

    public void Create(Notification notification) => _db.Notifications.Add(notification);

    public void Update(Notification notification) => _db.Notifications.Update(notification);

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (notification is null)
        {
            return false;
        }

        _db.Notifications.Remove(notification);
        return true;
    }

    public async Task<int> DeleteAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _db.Notifications
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0)
        {
            return 0;
        }

        _db.Notifications.RemoveRange(notifications);
        return notifications.Count;
    }

    public async Task<bool> MarkAsReadAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

        if (notification is null)
        {
            return false;
        }

        if (notification.IsRead)
        {
            return true;
        }

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unread = await _db.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        return unread.Count;
    }

    public Task<Notification?> FindByReferenceAsync(
        Guid userId,
        string type,
        Guid referenceId,
        CancellationToken cancellationToken = default) =>
        _db.Notifications.FirstOrDefaultAsync(
            x => x.UserId == userId && x.Type == type && x.ReferenceId == referenceId,
            cancellationToken);

    public Task<UserNotificationSettings?> GetSettingsAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        _db.UserNotificationSettings.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public void CreateSettings(UserNotificationSettings settings) =>
        _db.UserNotificationSettings.Add(settings);

    public void UpdateSettings(UserNotificationSettings settings) =>
        _db.UserNotificationSettings.Update(settings);
}
