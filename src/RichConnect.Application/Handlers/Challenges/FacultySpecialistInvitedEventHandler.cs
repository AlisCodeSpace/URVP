using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyFacultySpecialistInvited;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers
{
    /// <summary>
    /// Event handler for FacultySpecialistInvitedEvent
    /// </summary>
    public class FacultySpecialistInvitedEventHandler : IEventHandler<FacultySpecialistInvitedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FacultySpecialistInvitedEventHandler> _logger;

        public FacultySpecialistInvitedEventHandler(
            IMediator mediator,
            ILogger<FacultySpecialistInvitedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(FacultySpecialistInvitedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling FacultySpecialistInvitedEvent for invite {InviteId}, challenge {ChallengeId}, facultySpecialist {facultySpecialistId}", 
                    domainEvent.InviteId, domainEvent.ChallengeId, domainEvent.FacultySpecialistUserId);

                // Send notification to facultySpecialist using CQRS
                await _mediator.Send(new NotifyFacultySpecialistInvitedCommand
                {
                    InviteId = domainEvent.InviteId,
                    ChallengeId = domainEvent.ChallengeId,
                    FacultySpecialistUserId = domainEvent.FacultySpecialistUserId,
                    FacultySpecialistName = domainEvent.FacultySpecialistName,
                    ChallengeTitle = domainEvent.ChallengeTitle,
                    ThemeName = domainEvent.ThemeName,
                    PartnerName = domainEvent.PartnerName,
                    ChallengeDescription = domainEvent.ChallengeDescription
                });

                _logger.LogInformation("Successfully processed FacultySpecialistInvitedEvent for invite {InviteId}. " +
                    "Notified facultySpecialist {FacultySpecialistName}", domainEvent.InviteId, domainEvent.FacultySpecialistName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling FacultySpecialistInvitedEvent for invite {InviteId}", 
                    domainEvent.InviteId);
                throw;
            }
        }
    }
}
