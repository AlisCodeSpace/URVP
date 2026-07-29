using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyFacultySpecialistInvited
{
    public class NotifyFacultySpecialistInvitedCommandHandler : BaseCommandHandler<NotifyFacultySpecialistInvitedCommand>
    {
        private readonly IMediator _mediator;

        public NotifyFacultySpecialistInvitedCommandHandler(
            IMediator mediator,
            ILogger<NotifyFacultySpecialistInvitedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyFacultySpecialistInvitedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyFacultySpecialistInvitedCommand for invite {InviteId}, challenge {ChallengeId}, facultySpecialist {facultySpecialistId}", 
                request.InviteId, request.ChallengeId, request.FacultySpecialistUserId);

            try
            {
                // Create in-app notification for facultySpecialist using MediatR
                var command = new CreateNotificationCommand
                {
                    UserId = request.FacultySpecialistUserId,
                    Title = NotificationMessages.FacultySpecialist.InvitedTitle(),
                    Message = NotificationMessages.FacultySpecialist.InvitedMessage(request.ChallengeTitle, request.PartnerName, request.ChallengeDescription),
                    Type = NotificationType.FacultySpecialistInvited,
                    Link = $"/challenges/{request.ChallengeId}",
                    Priority = "medium",
                    ReferenceId = request.InviteId,
                    ReferenceType = "Invite"
                };

                await _mediator.Send(command, cancellationToken);

                // NOTE: Email sending is now handled by the NotificationCreatedEventHandler
                // which queues the email in the NotificationOutbox for reliable delivery.
                // The direct email sending has been removed to prevent duplicate emails.

                _logger.LogInformation("Successfully created notification for facultySpecialist invitation {InviteId}", request.InviteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for facultySpecialist invitation {InviteId}", request.InviteId);
                throw;
            }
        }
    }
}
