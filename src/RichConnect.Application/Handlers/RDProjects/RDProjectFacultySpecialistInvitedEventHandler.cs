using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectFacultySpecialistInvited;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.RDProjects
{
    public class RDProjectFacultySpecialistInvitedEventHandler : IEventHandler<RDProjectFacultySpecialistInvitedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RDProjectFacultySpecialistInvitedEventHandler> _logger;

        public RDProjectFacultySpecialistInvitedEventHandler(
            IMediator mediator,
            ILogger<RDProjectFacultySpecialistInvitedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(RDProjectFacultySpecialistInvitedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling RDProjectFacultySpecialistInvitedEvent for invite {InviteId}, R&D project {ProjectId}, facultySpecialist {facultySpecialistId}", 
                    domainEvent.InviteId, domainEvent.RDProjectId, domainEvent.FacultySpecialistUserId);

                // Send notification to facultySpecialist using CQRS
                await _mediator.Send(new NotifyRDProjectFacultySpecialistInvitedCommand
                {
                    InviteId = domainEvent.InviteId,
                    RDProjectId = domainEvent.RDProjectId,
                    FacultySpecialistUserId = domainEvent.FacultySpecialistUserId,
                    FacultySpecialistName = domainEvent.FacultySpecialistName,
                    ProjectTitle = domainEvent.ProjectTitle
                });

                _logger.LogInformation("Successfully processed RDProjectFacultySpecialistInvitedEvent for invite {InviteId}. " +
                    "Notified facultySpecialist {FacultySpecialistName}", domainEvent.InviteId, domainEvent.FacultySpecialistName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling RDProjectFacultySpecialistInvitedEvent for invite {InviteId}", 
                    domainEvent.InviteId);
                throw;
            }
        }
    }
}
