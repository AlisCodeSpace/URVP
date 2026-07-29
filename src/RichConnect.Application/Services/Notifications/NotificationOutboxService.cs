using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Application.Interfaces.Analytics;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Notifications.Interfaces;
using RICHConnect.Backend.Domain.Entities.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Services.Notifications
{
    public class NotificationOutboxService
    {
        private const string PortalSignInUrl = "https://richconnect.aub.edu.lb/sign-in/";
        private const string PortalActionText = "Open Portal";

        private readonly INotificationOutboxRepository _outboxRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailService _emailService;
        private readonly IUserEmailService _userEmailService;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<NotificationOutboxService> _logger;
        
        private const int MaxRetries = 5;
        
        public NotificationOutboxService(
            INotificationOutboxRepository outboxRepository,
            INotificationRepository notificationRepository,
            IEmailService emailService,
            IUserEmailService userEmailService,
            IAnalyticsService analyticsService,
            ILogger<NotificationOutboxService> logger)
        {
            _outboxRepository = outboxRepository;
            _notificationRepository = notificationRepository;
            _emailService = emailService;
            _userEmailService = userEmailService;
            _analyticsService = analyticsService;
            _logger = logger;
        }
        
        /// <summary>
        /// Processes pending outbox items
        /// </summary>
        public async Task ProcessOutboxAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting notification outbox processing");
                
                var pendingItems = await _outboxRepository.GetPendingItemsAsync(50);
                
                _logger.LogInformation("Found {Count} pending notification outbox items to process", pendingItems.Count);
                
                foreach (var item in pendingItems)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Cancellation requested, stopping outbox processing");
                        break;
                    }
                    
                    try
                    {
                        // Mark as processing
                        await _outboxRepository.UpdateStatusAsync(item.Id, "Processing");
                        
                        // Process based on event type
                        switch (item.EventType)
                        {
                            case "EmailNotification":
                                await ProcessEmailNotificationAsync(item);
                                break;
                                
                            // Add more event types here as needed
                            
                            default:
                                _logger.LogWarning("Unknown event type {EventType} for outbox item {OutboxId}", 
                                    item.EventType, item.Id);
                                await _outboxRepository.UpdateStatusAsync(item.Id, "Failed", 
                                    $"Unknown event type: {item.EventType}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        await HandleRetryAsync(item, ex);
                    }
                }
                
                _logger.LogDebug("Completed notification outbox processing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification outbox");
            }
        }
        
        /// <summary>
        /// Processes an email notification outbox item
        /// </summary>
        private async Task ProcessEmailNotificationAsync(NotificationOutbox item)
        {
            _logger.LogDebug("Processing email notification outbox item {OutboxId}", item.Id);
            
            // Get the notification
            var notification = await _notificationRepository.GetByIdAsync(item.NotificationId);
            
            if (notification == null)
            {
                _logger.LogWarning("Notification {NotificationId} not found for outbox item {OutboxId}", 
                    item.NotificationId, item.Id);
                await _outboxRepository.UpdateStatusAsync(item.Id, "Failed", 
                    "Notification not found");
                return;
            }
            
            _logger.LogInformation("Processing email notification for user {UserId}, notification {NotificationId}", 
                notification.UserId, notification.Id);
            
            // Get user email
            var userEmail = await _userEmailService.GetUserEmailAsync(notification.UserId);
            var userName = await _userEmailService.GetUserNameAsync(notification.UserId);
            
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("User email not found for user {UserId}, outbox item {OutboxId}", 
                    notification.UserId, item.Id);
                await _outboxRepository.UpdateStatusAsync(item.Id, "Failed", 
                    "User email not found");
                await _analyticsService.TrackNotificationEventAsync("email_skipped", notification.UserId, new Dictionary<string, object>
                {
                    { "notificationId", notification.Id },
                    { "notificationType", notification.Type },
                    { "reason", "user_email_not_found" }
                });
                return;
            }
            
            _logger.LogInformation("Sending email to {UserEmail} for notification {NotificationId}", 
                userEmail, notification.Id);
            
            var includePortalButton =
                notification.Type == NotificationType.FacultySpecialistInvited.ToString();

            var success = await _emailService.SendEmailAsync(
                userEmail,
                userName ?? "User",
                notification.Title,
                notification.Message,
                includePortalButton ? PortalSignInUrl : null,
                includePortalButton ? PortalActionText : null);
            
            if (success)
            {
                _logger.LogInformation("Successfully sent email notification for outbox item {OutboxId} to {UserEmail}", 
                    item.Id, userEmail);
                await _outboxRepository.UpdateStatusAsync(item.Id, "Completed");
                await _analyticsService.TrackNotificationEventAsync("email_sent", notification.UserId, new Dictionary<string, object>
                {
                    { "notificationId", notification.Id },
                    { "notificationType", notification.Type },
                    { "outboxId", item.Id }
                });
            }
            else
            {
                _logger.LogError("Failed to send email notification for outbox item {OutboxId} to {UserEmail}", 
                    item.Id, userEmail);
                await _analyticsService.TrackNotificationEventAsync("email_send_failed", notification.UserId, new Dictionary<string, object>
                {
                    { "notificationId", notification.Id },
                    { "notificationType", notification.Type },
                    { "outboxId", item.Id }
                });
                throw new InvalidOperationException($"Failed to send email notification to {userEmail}");
            }
        }
        
        /// <summary>
        /// Handles retry logic for failed outbox items
        /// </summary>
        private async Task HandleRetryAsync(NotificationOutbox item, Exception ex)
        {
            _logger.LogWarning(ex, "Error processing notification outbox item {OutboxId}", item.Id);
            
            if (item.RetryCount >= MaxRetries)
            {
                _logger.LogError("Max retries reached for notification outbox item {OutboxId}, marking as failed", item.Id);
                await _outboxRepository.UpdateStatusAsync(item.Id, "Failed", 
                    $"Max retries reached: {ex.Message}");
            }
            else
            {
                // Exponential backoff: 1min, 2min, 4min, 8min, 16min
                var delayMinutes = Math.Pow(2, item.RetryCount);
                var nextRetry = DateTime.UtcNow.AddMinutes(delayMinutes);
                
                _logger.LogInformation("Scheduling retry {RetryCount}/{MaxRetries} for notification outbox item {OutboxId} at {NextRetry}", 
                    item.RetryCount + 1, MaxRetries, item.Id, nextRetry);
                
                await _outboxRepository.IncrementRetryAsync(item.Id, nextRetry);
            }
        }
        
        /// <summary>
        /// Queues an email notification in the outbox
        /// </summary>
        public async Task QueueEmailNotificationAsync(Guid notificationId)
        {
            _logger.LogDebug("Queueing email notification for notification {NotificationId}", notificationId);
            
            var outboxItem = new NotificationOutbox
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                EventType = "EmailNotification",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            
            await _outboxRepository.CreateAsync(outboxItem);
            
            _logger.LogInformation("Queued email notification {OutboxId} for notification {NotificationId}", 
                outboxItem.Id, notificationId);
        }
    }
}
