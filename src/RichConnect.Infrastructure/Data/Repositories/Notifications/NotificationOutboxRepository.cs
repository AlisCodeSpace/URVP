using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications
{
    public class NotificationOutboxRepository : INotificationOutboxRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NotificationOutboxRepository> _logger;

        public NotificationOutboxRepository(
            AppDbContext context,
            ILogger<NotificationOutboxRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NotificationOutbox> CreateAsync(NotificationOutbox outboxItem)
        {
            _logger.LogDebug("Creating notification outbox item for notification {NotificationId}", outboxItem.NotificationId);
            
            _context.NotificationOutbox.Add(outboxItem);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created notification outbox item {OutboxId} for notification {NotificationId}", 
                outboxItem.Id, outboxItem.NotificationId);
            
            return outboxItem;
        }

        public async Task<List<NotificationOutbox>> GetPendingItemsAsync(int batchSize = 50)
        {
            _logger.LogDebug("Getting pending notification outbox items (batch size: {BatchSize})", batchSize);
            
            var now = DateTime.UtcNow;
            
            var pendingItems = await _context.NotificationOutbox
                .AsNoTracking()
                .Where(o => 
                    (o.Status == "Pending" || o.Status == "Processing") && 
                    (o.NextRetryAt == null || o.NextRetryAt <= now))
                .OrderBy(o => o.CreatedAt)
                .Take(batchSize)
                .ToListAsync();
            
            _logger.LogDebug("Found {Count} pending notification outbox items", pendingItems.Count);
            
            return pendingItems;
        }

        public async Task UpdateStatusAsync(Guid id, string status, string? errorMessage = null)
        {
            _logger.LogDebug("Updating status of notification outbox item {OutboxId} to {Status}", id, status);
            
            var outboxItem = await _context.NotificationOutbox.FindAsync(id);
            
            if (outboxItem == null)
            {
                _logger.LogWarning("Notification outbox item {OutboxId} not found", id);
                return;
            }
            
            outboxItem.Status = status;
            
            if (errorMessage != null)
            {
                outboxItem.ErrorMessage = errorMessage;
            }
            
            if (status == "Completed" || status == "Failed")
            {
                outboxItem.ProcessedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated status of notification outbox item {OutboxId} to {Status}", id, status);
        }

        public async Task IncrementRetryAsync(Guid id, DateTime nextRetryAt)
        {
            _logger.LogDebug("Incrementing retry count for notification outbox item {OutboxId}", id);
            
            var outboxItem = await _context.NotificationOutbox.FindAsync(id);
            
            if (outboxItem == null)
            {
                _logger.LogWarning("Notification outbox item {OutboxId} not found", id);
                return;
            }
            
            outboxItem.RetryCount++;
            outboxItem.NextRetryAt = nextRetryAt;
            outboxItem.Status = "Pending"; // Reset to pending for next retry
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Incremented retry count for notification outbox item {OutboxId} to {RetryCount}, next retry at {NextRetryAt}", 
                id, outboxItem.RetryCount, nextRetryAt);
        }

        public async Task<NotificationOutbox?> GetByIdAsync(Guid id)
        {
            return await _context.NotificationOutbox
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<NotificationOutbox>> GetByNotificationIdAsync(Guid notificationId)
        {
            return await _context.NotificationOutbox
                .AsNoTracking()
                .Where(o => o.NotificationId == notificationId)
                .ToListAsync();
        }
    }
}
