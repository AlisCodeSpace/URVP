using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Interfaces.Notifications;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Handlers
{
    /// <summary>
    /// Event handler for ChallengeRejectedEvent
    /// </summary>
    public class ChallengeRejectedEventHandler : IEventHandler<ChallengeRejectedEvent>
    {
        private readonly INotificationApplicationService _notificationService;
        private readonly ILogger<ChallengeRejectedEventHandler> _logger;

        public ChallengeRejectedEventHandler(
            INotificationApplicationService notificationService,
            ILogger<ChallengeRejectedEventHandler> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task HandleAsync(ChallengeRejectedEvent domainEvent)
        {
            _logger.LogInformation("Handling ChallengeRejectedEvent for challenge {ChallengeId}", 
                domainEvent.ChallengeId);

            try
            {
                // Send in-app notification to challenge submitter
                var request = new CreateNotificationRequest
                {
                    UserId = domainEvent.SubmittedByUserId,
                    Title = NotificationMessages.Challenge.RejectedTitle(),
                    Message = NotificationMessages.Challenge.RejectedMessage(domainEvent.ChallengeTitle, domainEvent.RejectionReason),
                    Type = NotificationType.ChallengeRejected,
                    Link = $"/challenges/{domainEvent.ChallengeId}",
                    Priority = "high"
                };

                await _notificationService.CreateNotificationAsync(request);

                // NOTE: Email sending is now handled by the NotificationCreatedEventHandler
                // which queues the email in the NotificationOutbox for reliable delivery.
                // The direct email sending has been removed to prevent duplicate emails.

                _logger.LogInformation("Successfully processed ChallengeRejectedEvent for challenge {ChallengeId}. " +
                    "Notified submitter {SubmitterName} with reason: {RejectionReason}", 
                    domainEvent.ChallengeId, domainEvent.SubmittedByName, domainEvent.RejectionReason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ChallengeRejectedEvent for challenge {ChallengeId}", 
                    domainEvent.ChallengeId);
                throw;
            }
        }
    }
}
