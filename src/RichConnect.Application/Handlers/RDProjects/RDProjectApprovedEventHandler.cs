using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectApproved;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.RDProjects
{
    public class RDProjectApprovedEventHandler : IEventHandler<RDProjectApprovedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RDProjectApprovedEventHandler> _logger;

        public RDProjectApprovedEventHandler(
            IMediator mediator,
            ILogger<RDProjectApprovedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(RDProjectApprovedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling RDProjectApprovedEvent for R&D project {ProjectId} approved by {ApprovedBy}", 
                    domainEvent.RDProjectId, domainEvent.ApprovedByName);

                // Send approval notification to submitter using CQRS
                await _mediator.Send(new NotifyRDProjectApprovedCommand
                {
                    RDProjectId = domainEvent.RDProjectId,
                    ApprovedByUserId = domainEvent.ApprovedByUserId
                });

                _logger.LogInformation("Successfully processed RDProjectApprovedEvent for R&D project {ProjectId}. " +
                    "Notification sent to submitter {SubmitterName}", domainEvent.RDProjectId, domainEvent.SubmittedByName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling RDProjectApprovedEvent for R&D project {ProjectId}", 
                    domainEvent.RDProjectId);
                throw;
            }
        }
    }
}
