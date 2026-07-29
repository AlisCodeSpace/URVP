using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Application.Interfaces.Analytics;

namespace RICHConnect.Backend.Application.Handlers.Notifications
{
    public class NotificationDeletedEventHandler : IEventHandler<NotificationDeletedEvent>
    {
        private readonly ILogger<NotificationDeletedEventHandler> _logger;
        private readonly IEventBus _eventBus;
        private readonly INotificationCacheService _cacheService;
        private readonly IAnalyticsService _analyticsService;
        
        public NotificationDeletedEventHandler(
            ILogger<NotificationDeletedEventHandler> logger,
            IEventBus eventBus,
            INotificationCacheService cacheService,
            IAnalyticsService analyticsService)
        {
            _logger = logger;
            _eventBus = eventBus;
            _cacheService = cacheService;
            _analyticsService = analyticsService;
        }
        
        public async Task HandleAsync(NotificationDeletedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling notification deleted event for notification {NotificationId} and user {UserId}", 
                    domainEvent.NotificationId, domainEvent.UserId);

                // Log notification deletion for audit trail
                _logger.LogInformation("User {UserId} deleted notification {NotificationId}", 
                    domainEvent.UserId, domainEvent.NotificationId);

                // Invalidate cache to force refresh on next read
                try
                {
                    await _cacheService.InvalidateUnreadCountAsync(domainEvent.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to invalidate notification count cache for user {UserId}", domainEvent.UserId);
                }

                // Track analytics event
                try
                {
                    await _analyticsService.TrackNotificationEventAsync("notification_deleted", domainEvent.UserId, new Dictionary<string, object>
                    {
                        { "notificationId", domainEvent.NotificationId }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to track analytics for notification deletion {NotificationId}", domainEvent.NotificationId);
                }

                _logger.LogInformation("Successfully handled notification deleted event for notification {NotificationId}", 
                    domainEvent.NotificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling notification deleted event for notification {NotificationId}", 
                    domainEvent.NotificationId);
                // Don't rethrow - event handler failures shouldn't break the main flow
            }
        }
    }
}

