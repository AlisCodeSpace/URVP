using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectRejected;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.RDProjects
{
    public class RDProjectRejectedEventHandler : IEventHandler<RDProjectRejectedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RDProjectRejectedEventHandler> _logger;

        public RDProjectRejectedEventHandler(
            IMediator mediator,
            ILogger<RDProjectRejectedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(RDProjectRejectedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling RDProjectRejectedEvent for R&D project {ProjectId} rejected by {RejectedBy}", 
                    domainEvent.RDProjectId, domainEvent.RejectedByName);

                // Send rejection notification to submitter using CQRS
                await _mediator.Send(new NotifyRDProjectRejectedCommand
                {
                    RDProjectId = domainEvent.RDProjectId,
                    RejectedByUserId = domainEvent.RejectedByUserId,
                    RejectionReason = domainEvent.RejectionReason
                });

                _logger.LogInformation("Successfully processed RDProjectRejectedEvent for R&D project {ProjectId}. " +
                    "Notification sent to submitter {SubmitterName}", domainEvent.RDProjectId, domainEvent.SubmittedByName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling RDProjectRejectedEvent for R&D project {ProjectId}", 
                    domainEvent.RDProjectId);
                throw;
            }
        }
    }
}
