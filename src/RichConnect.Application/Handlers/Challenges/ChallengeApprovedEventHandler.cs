using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyChallengeApproved;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers
{
    /// <summary>
    /// Event handler for ChallengeApprovedEvent
    /// </summary>
    public class ChallengeApprovedEventHandler : IEventHandler<ChallengeApprovedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ChallengeApprovedEventHandler> _logger;

        public ChallengeApprovedEventHandler(
            IMediator mediator,
            ILogger<ChallengeApprovedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(ChallengeApprovedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ChallengeApprovedEvent for challenge {ChallengeId} approved by {ApprovedBy}", 
                    domainEvent.ChallengeId, domainEvent.ApprovedByName);

                // Send approval notification to submitter using CQRS
                // This will create an in-app notification AND queue an email notification automatically
                await _mediator.Send(new NotifyChallengeApprovedCommand
                {
                    ChallengeId = domainEvent.ChallengeId,
                    ApprovedByUserId = domainEvent.ApprovedByUserId
                });

                // NOTE: Email sending is now handled by the NotificationCreatedEventHandler
                // which queues the email in the NotificationOutbox for reliable delivery.
                // The direct email sending has been removed to prevent duplicate emails.

                _logger.LogInformation("Successfully processed ChallengeApprovedEvent for challenge {ChallengeId}. " +
                    "Notification sent to submitter {SubmitterName}", domainEvent.ChallengeId, domainEvent.SubmittedByName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ChallengeApprovedEvent for challenge {ChallengeId}", 
                    domainEvent.ChallengeId);
                throw;
            }
        }
    }
}
