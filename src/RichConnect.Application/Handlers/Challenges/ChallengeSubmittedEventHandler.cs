using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyChallengeSubmitted;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers
{
    /// <summary>
    /// Event handler for ChallengeSubmittedEvent
    /// </summary>
    public class ChallengeSubmittedEventHandler : IEventHandler<ChallengeSubmittedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ChallengeSubmittedEventHandler> _logger;

        public ChallengeSubmittedEventHandler(
            IMediator mediator,
            ILogger<ChallengeSubmittedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(ChallengeSubmittedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ChallengeSubmittedEvent for challenge {ChallengeId} submitted by {SubmittedBy}", 
                    domainEvent.ChallengeId, domainEvent.SubmittedByName);

                // Send notification to admins for review using CQRS
                await _mediator.Send(new NotifyChallengeSubmittedCommand
                {
                    ChallengeId = domainEvent.ChallengeId,
                    SubmittedByUserId = domainEvent.SubmittedByUserId
                });

                _logger.LogInformation("Successfully processed ChallengeSubmittedEvent for challenge {ChallengeId}. " +
                    "Notification sent to admins", domainEvent.ChallengeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ChallengeSubmittedEvent for challenge {ChallengeId}", 
                    domainEvent.ChallengeId);
                throw;
            }
        }
    }
}
