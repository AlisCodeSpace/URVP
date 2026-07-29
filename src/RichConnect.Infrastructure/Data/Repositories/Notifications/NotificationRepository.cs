using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(AppDbContext context, ILogger<NotificationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page, int pageSize, bool? isRead = null)
        {
            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId);

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            
            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found for user {UserId} - potential IDOR attempt", id, userId);
                return false;
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Notification {NotificationId} deleted by owner {UserId}", id, userId);
            return true;
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<UserNotificationSettings?> GetUserSettingsAsync(Guid userId)
        {
            return await _context.UserNotificationSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task UpdateUserSettingsAsync(UserNotificationSettings settings)
        {
            var existingSettings = await _context.UserNotificationSettings
                .FirstOrDefaultAsync(s => s.UserId == settings.UserId);

            if (existingSettings != null)
            {
                existingSettings.EmailNotifications = settings.EmailNotifications;
                existingSettings.InAppNotifications = settings.InAppNotifications;
                existingSettings.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.UserNotificationSettings.Add(settings);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalCountAsync(Guid userId, bool? isRead = null)
        {
            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId);
            
            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);
            
            return await query.CountAsync();
        }
        
        public async Task<Notification?> FindByReferenceAsync(Guid userId, NotificationType type, Guid referenceId)
        {
            _logger.LogDebug("Finding notification by reference for user {UserId}, type {Type}, reference {ReferenceId}", 
                userId, type, referenceId);
            
            return await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => 
                    n.UserId == userId && 
                    n.Type == type.ToString() && 
                    n.ReferenceId == referenceId);
        }

        public async Task<UserNotificationSettings?> GetUserNotificationSettingsAsync(Guid userId)
        {
            return await _context.UserNotificationSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task CreateUserNotificationSettingsAsync(UserNotificationSettings settings)
        {
            _context.UserNotificationSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
    }
}

