using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Handlers.Challenges
{
    /// <summary>
    /// Event handler for ChallengeEditRequestApprovedEvent
    /// Handles notifications when an edit request is approved by an admin
    /// </summary>
    public class ChallengeEditRequestApprovedEventHandler : IEventHandler<ChallengeEditRequestApprovedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ChallengeEditRequestApprovedEventHandler> _logger;

        public ChallengeEditRequestApprovedEventHandler(
            IMediator mediator,
            ILogger<ChallengeEditRequestApprovedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(ChallengeEditRequestApprovedEvent domainEvent)
        {
            _logger.LogInformation("Handling ChallengeEditRequestApprovedEvent for EditRequestId: {EditRequestId}, ChallengeId: {ChallengeId}", 
                domainEvent.EditRequestId, domainEvent.ChallengeId);

            try
            {
                // Send notification to the Community Partner who requested the edit
                var command = new CreateNotificationCommand
                {
                    UserId = domainEvent.RequestedBy,
                    Title = NotificationMessages.Challenge.EditRequestApprovedTitle(),
                    Message = NotificationMessages.Challenge.EditRequestApprovedMessage(domainEvent.AdminResponse),
                    Type = NotificationType.ChallengeEditRequestApproved,
                    Link = $"/challenges/{domainEvent.ChallengeId}",
                    Priority = "medium",
                    ReferenceId = domainEvent.EditRequestId,
                    ReferenceType = "ChallengeEditRequest"
                };
                
                var notificationId = await _mediator.Send(command);
                _logger.LogInformation("Created edit request approved notification {NotificationId} for user {UserId}", 
                    notificationId, domainEvent.RequestedBy);

                _logger.LogInformation("Successfully processed ChallengeEditRequestApprovedEvent for EditRequestId: {EditRequestId}", 
                    domainEvent.EditRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ChallengeEditRequestApprovedEvent for EditRequestId: {EditRequestId}", 
                    domainEvent.EditRequestId);
                throw;
            }
        }
    }
}
