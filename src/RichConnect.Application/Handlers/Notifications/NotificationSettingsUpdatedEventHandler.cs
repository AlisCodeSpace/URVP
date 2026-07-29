using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.Notifications
{
    public class NotificationSettingsUpdatedEventHandler : IEventHandler<NotificationSettingsUpdatedEvent>
    {
        private readonly ILogger<NotificationSettingsUpdatedEventHandler> _logger;
        
        public NotificationSettingsUpdatedEventHandler(
            ILogger<NotificationSettingsUpdatedEventHandler> logger)
        {
            _logger = logger;
        }
        
        public Task HandleAsync(NotificationSettingsUpdatedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling notification settings updated event for user {UserId}", 
                    domainEvent.UserId);

                // Log settings update for audit trail
                _logger.LogInformation("User {UserId} updated notification settings - Email: {Email}, InApp: {InApp}", 
                    domainEvent.UserId, 
                    domainEvent.EmailNotifications, 
                    domainEvent.InAppNotifications);

                // Note: Notification settings are stored in database and retrieved on each notification creation
                // Caching can be added later if performance becomes an issue

                _logger.LogInformation("Successfully handled notification settings updated event for user {UserId}", 
                    domainEvent.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling notification settings updated event for user {UserId}", 
                    domainEvent.UserId);
                // Don't rethrow - event handler failures shouldn't break the main flow
            }
            
            return Task.CompletedTask;
        }
    }
}

