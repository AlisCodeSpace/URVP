using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.DomainEvents.NotifyRDProjectSubmitted;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.RDProjects
{
    public class RDProjectSubmittedEventHandler : IEventHandler<RDProjectSubmittedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RDProjectSubmittedEventHandler> _logger;

        public RDProjectSubmittedEventHandler(
            IMediator mediator,
            ILogger<RDProjectSubmittedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(RDProjectSubmittedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling RDProjectSubmittedEvent for project {ProjectId} submitted by {SubmittedBy}", 
                    domainEvent.RDProjectId, domainEvent.SubmittedByName);

                // Send notification to admins for review
                await _mediator.Send(new NotifyRDProjectSubmittedCommand
                {
                    RDProjectId = domainEvent.RDProjectId,
                    SubmittedByUserId = domainEvent.SubmittedByUserId
                });

                _logger.LogInformation("Successfully processed RDProjectSubmittedEvent for project {ProjectId}. " +
                    "Notification sent to admins", domainEvent.RDProjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling RDProjectSubmittedEvent for project {ProjectId}", 
                    domainEvent.RDProjectId);
                throw;
            }
        }
    }
}
