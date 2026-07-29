using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Application.Interfaces.Analytics;

namespace RICHConnect.Backend.Application.Handlers.Notifications
{
    public class NotificationReadEventHandler : IEventHandler<NotificationReadEvent>
    {
        private readonly ILogger<NotificationReadEventHandler> _logger;
        private readonly IEventBus _eventBus;
        private readonly INotificationCacheService _cacheService;
        private readonly IAnalyticsService _analyticsService;
        
        public NotificationReadEventHandler(
            ILogger<NotificationReadEventHandler> logger,
            IEventBus eventBus,
            INotificationCacheService cacheService,
            IAnalyticsService analyticsService)
        {
            _logger = logger;
            _eventBus = eventBus;
            _cacheService = cacheService;
            _analyticsService = analyticsService;
        }
        
        public async Task HandleAsync(NotificationReadEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling notification read event for notification {NotificationId} and user {UserId}", 
                    domainEvent.NotificationId, domainEvent.UserId);

                // Log notification read for analytics
                _logger.LogInformation("User {UserId} read notification {NotificationId} at {ReadAt}", 
                    domainEvent.UserId, domainEvent.NotificationId, domainEvent.ReadAt);

                // Update unread count cache
                try
                {
                    await _cacheService.DecrementUnreadCountAsync(domainEvent.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update notification count cache for user {UserId}", domainEvent.UserId);
                }

                // Trigger analytics event
                try
                {
                    await _analyticsService.TrackNotificationEventAsync("notification_read", domainEvent.UserId, new Dictionary<string, object>
                    {
                        { "notificationId", domainEvent.NotificationId },
                        { "readAt", domainEvent.ReadAt }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to track analytics for notification read {NotificationId}", domainEvent.NotificationId);
                }

                _logger.LogInformation("Successfully handled notification read event for notification {NotificationId}", 
                    domainEvent.NotificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling notification read event for notification {NotificationId}", 
                    domainEvent.NotificationId);
                // Don't rethrow - event handler failures shouldn't break the main flow
            }
        }
    }
}

