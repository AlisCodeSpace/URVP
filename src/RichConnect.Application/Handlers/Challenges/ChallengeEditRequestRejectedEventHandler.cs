using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Handlers.Challenges
{
    /// <summary>
    /// Event handler for ChallengeEditRequestRejectedEvent
    /// Handles notifications when an edit request is rejected by an admin
    /// </summary>
    public class ChallengeEditRequestRejectedEventHandler : IEventHandler<ChallengeEditRequestRejectedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ChallengeEditRequestRejectedEventHandler> _logger;

        public ChallengeEditRequestRejectedEventHandler(
            IMediator mediator,
            ILogger<ChallengeEditRequestRejectedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(ChallengeEditRequestRejectedEvent domainEvent)
        {
            _logger.LogInformation("Handling ChallengeEditRequestRejectedEvent for EditRequestId: {EditRequestId}, ChallengeId: {ChallengeId}", 
                domainEvent.EditRequestId, domainEvent.ChallengeId);

            try
            {
                // Send notification to the Community Partner who requested the edit
                var command = new CreateNotificationCommand
                {
                    UserId = domainEvent.RequestedBy,
                    Title = NotificationMessages.Challenge.EditRequestRejectedTitle(),
                    Message = NotificationMessages.Challenge.EditRequestRejectedMessage(domainEvent.AdminResponse),
                    Type = NotificationType.ChallengeEditRequestRejected,
                    Link = $"/challenges/{domainEvent.ChallengeId}",
                    Priority = "medium",
                    ReferenceId = domainEvent.EditRequestId,
                    ReferenceType = "ChallengeEditRequest"
                };
                
                var notificationId = await _mediator.Send(command);
                _logger.LogInformation("Created edit request rejected notification {NotificationId} for user {UserId}", 
                    notificationId, domainEvent.RequestedBy);

                _logger.LogInformation("Successfully processed ChallengeEditRequestRejectedEvent for EditRequestId: {EditRequestId}", 
                    domainEvent.EditRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ChallengeEditRequestRejectedEvent for EditRequestId: {EditRequestId}", 
                    domainEvent.EditRequestId);
                throw;
            }
        }
    }
}
