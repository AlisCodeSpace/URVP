using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Application.Interfaces.Analytics;

namespace RICHConnect.Backend.Application.Handlers.Notifications
{
    public class NotificationCreatedEventHandler : IEventHandler<NotificationCreatedEvent>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly NotificationOutboxService _outboxService;
        private readonly NotificationBusinessRulesService _businessRulesService;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly INotificationCacheService _cacheService;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<NotificationCreatedEventHandler> _logger;
        
        public NotificationCreatedEventHandler(
            INotificationRepository notificationRepository,
            NotificationOutboxService outboxService,
            NotificationBusinessRulesService businessRulesService,
            IPushNotificationService pushNotificationService,
            INotificationCacheService cacheService,
            IAnalyticsService analyticsService,
            ILogger<NotificationCreatedEventHandler> logger)
        {
            _notificationRepository = notificationRepository;
            _outboxService = outboxService;
            _businessRulesService = businessRulesService;
            _pushNotificationService = pushNotificationService;
            _cacheService = cacheService;
            _analyticsService = analyticsService;
            _logger = logger;
        }
        
        public async Task HandleAsync(NotificationCreatedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling notification created event for notification {NotificationId} and user {UserId}", 
                    domainEvent.NotificationId, domainEvent.UserId);

                // Check if email should be sent (respects rate limits, user preferences, etc.)
                var shouldSendEmail = await _businessRulesService.ShouldSendEmailNotification(
                    domainEvent.UserId, 
                    domainEvent.Type.ToString());

                await _analyticsService.TrackNotificationEventAsync("notification_intended", domainEvent.UserId, new Dictionary<string, object>
                {
                    { "notificationId", domainEvent.NotificationId },
                    { "notificationType", domainEvent.Type.ToString() },
                    { "channel", "email" }
                });

                if (shouldSendEmail && !string.IsNullOrEmpty(domainEvent.Title))
                {
                    try
                    {
                        // Queue the notification in the outbox for reliable delivery
                        await _outboxService.QueueEmailNotificationAsync(domainEvent.NotificationId);
                        
                        _logger.LogInformation("Email notification queued for notification {NotificationId} to user {UserId}", 
                            domainEvent.NotificationId, domainEvent.UserId);

                        await _analyticsService.TrackNotificationEventAsync("email_queued", domainEvent.UserId, new Dictionary<string, object>
                        {
                            { "notificationId", domainEvent.NotificationId },
                            { "notificationType", domainEvent.Type.ToString() }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to queue email notification for notification {NotificationId} to user {UserId}", 
                            domainEvent.NotificationId, domainEvent.UserId);
                        await _analyticsService.TrackNotificationEventAsync("email_queue_failed", domainEvent.UserId, new Dictionary<string, object>
                        {
                            { "notificationId", domainEvent.NotificationId },
                            { "notificationType", domainEvent.Type.ToString() },
                            { "error", ex.Message }
                        });
                        // Don't throw - email queuing failure shouldn't break the notification creation
                    }
                }
                else
                {
                    _logger.LogInformation("Email notification not sent for user {UserId} - rate limit or preferences", 
                        domainEvent.UserId);
                    await _analyticsService.TrackNotificationEventAsync("email_skipped", domainEvent.UserId, new Dictionary<string, object>
                    {
                        { "notificationId", domainEvent.NotificationId },
                        { "notificationType", domainEvent.Type.ToString() },
                        { "reason", "rate_limit_or_preferences_or_missing_title" }
                    });
                }

                // Check if push notification should be sent
                var shouldSendPush = await _businessRulesService.ShouldSendPushNotification(
                    domainEvent.UserId, 
                    domainEvent.Type.ToString());

                if (shouldSendPush)
                {
                    try
                    {
                        await _pushNotificationService.QueuePushNotificationAsync(domainEvent.NotificationId);
                        _logger.LogInformation("Push notification queued for notification {NotificationId} to user {UserId}", 
                            domainEvent.NotificationId, domainEvent.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to queue push notification for notification {NotificationId}", 
                            domainEvent.NotificationId);
                        // Don't throw - push notification failure shouldn't break the notification creation
                    }
                }

                // Update unread count cache
                try
                {
                    await _cacheService.IncrementUnreadCountAsync(domainEvent.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update notification count cache for user {UserId}", domainEvent.UserId);
                }

                // Track analytics event
                try
                {
                    await _analyticsService.TrackNotificationEventAsync("notification_created", domainEvent.UserId, new Dictionary<string, object>
                    {
                        { "notificationId", domainEvent.NotificationId },
                        { "notificationType", domainEvent.Type.ToString() },
                        { "priority", shouldSendEmail || shouldSendPush ? "high" : "low" }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to track analytics for notification {NotificationId}", domainEvent.NotificationId);
                }

                _logger.LogInformation("Successfully handled notification created event for notification {NotificationId}", 
                    domainEvent.NotificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling notification created event for notification {NotificationId}", 
                    domainEvent.NotificationId);
                // Don't rethrow - event handler failures shouldn't break the main flow
            }
        }
    }
}

