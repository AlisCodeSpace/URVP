using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectFacultySpecialistResponded;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.RDProjects
{
    public class RDProjectFacultySpecialistRespondedEventHandler : IEventHandler<RDProjectFacultySpecialistRespondedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RDProjectFacultySpecialistRespondedEventHandler> _logger;

        public RDProjectFacultySpecialistRespondedEventHandler(
            IMediator mediator,
            ILogger<RDProjectFacultySpecialistRespondedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(RDProjectFacultySpecialistRespondedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling RDProjectFacultySpecialistRespondedEvent for invite {InviteId}, R&D project {ProjectId}, facultySpecialist {facultySpecialistId}, response: {Response}", 
                    domainEvent.InviteId, domainEvent.RDProjectId, domainEvent.FacultySpecialistUserId, domainEvent.ResponseText);

                // Send notification to admins using CQRS
                await _mediator.Send(new NotifyRDProjectFacultySpecialistRespondedCommand
                {
                    InviteId = domainEvent.InviteId,
                    RDProjectId = domainEvent.RDProjectId,
                    FacultySpecialistUserId = domainEvent.FacultySpecialistUserId,
                    FacultySpecialistName = domainEvent.FacultySpecialistName,
                    ResponseText = domainEvent.ResponseText
                });

                _logger.LogInformation("Successfully processed RDProjectFacultySpecialistRespondedEvent for invite {InviteId}. " +
                    "Notification sent to admins about facultySpecialist {FacultySpecialistName} {Response} response", 
                    domainEvent.InviteId, domainEvent.FacultySpecialistName, domainEvent.ResponseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling RDProjectFacultySpecialistRespondedEvent for invite {InviteId}", 
                    domainEvent.InviteId);
                throw;
            }
        }
    }
}
