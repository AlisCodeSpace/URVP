using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectMatched;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.RDProjects
{
    public class RDProjectMatchedEventHandler : IEventHandler<RDProjectMatchedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RDProjectMatchedEventHandler> _logger;

        public RDProjectMatchedEventHandler(
            IMediator mediator,
            ILogger<RDProjectMatchedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(RDProjectMatchedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling RDProjectMatchedEvent for R&D project {ProjectId} with {ProfessorCount} professors", 
                    domainEvent.RDProjectId, domainEvent.TotalMatchesCreated);

                // Send matching notifications to admins and partner
                await _mediator.Send(new NotifyRDProjectMatchedCommand
                {
                    RDProjectId = domainEvent.RDProjectId,
                    SubmittedByUserId = domainEvent.SubmittedByUserId,
                    ProjectTitle = domainEvent.ProjectTitle,
                    MatchedFacultySpecialistNames = domainEvent.MatchedFacultySpecialistNames,
                    TotalMatchesCreated = domainEvent.TotalMatchesCreated
                });

                _logger.LogInformation("Successfully processed RDProjectMatchedEvent for R&D project {ProjectId}. " +
                    "Notified admins and partner", domainEvent.RDProjectId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling RDProjectMatchedEvent for R&D project {ProjectId}", 
                    domainEvent.RDProjectId);
                throw;
            }
        }
    }
}
