using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyFacultySpecialistResponded;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers
{
    /// <summary>
    /// Event handler for FacultySpecialistRespondedEvent
    /// </summary>
    public class FacultySpecialistRespondedEventHandler : IEventHandler<FacultySpecialistRespondedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FacultySpecialistRespondedEventHandler> _logger;

        public FacultySpecialistRespondedEventHandler(
            IMediator mediator,
            ILogger<FacultySpecialistRespondedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(FacultySpecialistRespondedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling FacultySpecialistRespondedEvent for invite {InviteId}, challenge {ChallengeId}, facultySpecialist {facultySpecialistId}, response: {Response}", 
                    domainEvent.InviteId, domainEvent.ChallengeId, domainEvent.FacultySpecialistUserId, domainEvent.ResponseText);

                // Send notification to admins using CQRS
                await _mediator.Send(new NotifyFacultySpecialistRespondedCommand
                {
                    InviteId = domainEvent.InviteId,
                    ChallengeId = domainEvent.ChallengeId,
                    FacultySpecialistUserId = domainEvent.FacultySpecialistUserId,
                    FacultySpecialistName = domainEvent.FacultySpecialistName,
                    ResponseText = domainEvent.ResponseText
                });

                _logger.LogInformation("Successfully processed FacultySpecialistRespondedEvent for invite {InviteId}. " +
                    "Notification sent to admins about facultySpecialist {FacultySpecialistName} {Response} response", 
                    domainEvent.InviteId, domainEvent.FacultySpecialistName, domainEvent.ResponseText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling FacultySpecialistRespondedEvent for invite {InviteId}", 
                    domainEvent.InviteId);
                throw;
            }
        }
    }
}
