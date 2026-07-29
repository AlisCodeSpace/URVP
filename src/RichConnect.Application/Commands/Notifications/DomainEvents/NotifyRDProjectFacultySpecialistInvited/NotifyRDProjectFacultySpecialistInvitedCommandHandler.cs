using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectFacultySpecialistInvited
{
    public class NotifyRDProjectFacultySpecialistInvitedCommandHandler : BaseCommandHandler<NotifyRDProjectFacultySpecialistInvitedCommand>
    {
        private readonly IMediator _mediator;

        public NotifyRDProjectFacultySpecialistInvitedCommandHandler(
            IMediator mediator,
            ILogger<NotifyRDProjectFacultySpecialistInvitedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyRDProjectFacultySpecialistInvitedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyRDProjectFacultySpecialistInvitedCommand for invite {InviteId}, R&D project {ProjectId}, facultySpecialist {facultySpecialistId}", 
                request.InviteId, request.RDProjectId, request.FacultySpecialistUserId);

            try
            {
                // Create in-app notification for facultySpecialist
                var command = new CreateNotificationCommand
                {
                    UserId = request.FacultySpecialistUserId,
                    Title = NotificationMessages.RDProject.FacultySpecialistInvitedTitle(),
                    Message = NotificationMessages.RDProject.FacultySpecialistInvitedMessage(request.ProjectTitle, request.ProjectDescription),
                    Type = NotificationType.RDProjectFacultySpecialistInvited,
                    Link = $"/rd-projects/{request.RDProjectId}",
                    Priority = "medium",
                    ReferenceId = request.InviteId,
                    ReferenceType = "RDProjectInvite"
                };

                await _mediator.Send(command, cancellationToken);

                _logger.LogInformation("Successfully created notification for R&D project facultySpecialist invitation {InviteId}", request.InviteId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for R&D project facultySpecialist invitation {InviteId}", request.InviteId);
                throw;
            }
        }
    }
}
