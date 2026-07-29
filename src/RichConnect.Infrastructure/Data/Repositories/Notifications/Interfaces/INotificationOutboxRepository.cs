using RICHConnect.Backend.Domain.Entities.Notifications;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces
{
    public interface INotificationOutboxRepository
    {
        /// <summary>
        /// Creates a new notification outbox item
        /// </summary>
        Task<NotificationOutbox> CreateAsync(NotificationOutbox outboxItem);
        
        /// <summary>
        /// Gets pending outbox items that are ready to be processed
        /// </summary>
        Task<List<NotificationOutbox>> GetPendingItemsAsync(int batchSize = 50);
        
        /// <summary>
        /// Updates the status of an outbox item
        /// </summary>
        Task UpdateStatusAsync(Guid id, string status, string? errorMessage = null);
        
        /// <summary>
        /// Increments the retry count and sets the next retry time
        /// </summary>
        Task IncrementRetryAsync(Guid id, DateTime nextRetryAt);
        
        /// <summary>
        /// Gets an outbox item by its ID
        /// </summary>
        Task<NotificationOutbox?> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Gets outbox items by notification ID
        /// </summary>
        Task<List<NotificationOutbox>> GetByNotificationIdAsync(Guid notificationId);
    }
}
