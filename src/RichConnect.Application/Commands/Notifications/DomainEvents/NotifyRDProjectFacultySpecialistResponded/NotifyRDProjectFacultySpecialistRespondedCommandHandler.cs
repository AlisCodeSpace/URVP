using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectFacultySpecialistResponded
{
    public class NotifyRDProjectFacultySpecialistRespondedCommandHandler : BaseCommandHandler<NotifyRDProjectFacultySpecialistRespondedCommand>
    {
        private readonly IRDProjectRepository _rdProjectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public NotifyRDProjectFacultySpecialistRespondedCommandHandler(
            IRDProjectRepository rdProjectRepository,
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<NotifyRDProjectFacultySpecialistRespondedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _rdProjectRepository = rdProjectRepository;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyRDProjectFacultySpecialistRespondedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyRDProjectFacultySpecialistRespondedCommand for invite {InviteId}, R&D project {ProjectId}", 
                request.InviteId, request.RDProjectId);

            var project = await _rdProjectRepository.GetByIdWithIncludesAsync(request.RDProjectId);

            if (project == null)
            {
                _logger.LogWarning("R&D project {ProjectId} not found for notification", request.RDProjectId);
                return;
            }

            // Get all admin users
            var adminUsers = await _userRepository.GetAdminUserIdsAsync();

            if (!adminUsers.Any())
            {
                _logger.LogWarning("No admin users found for R&D project facultySpecialist response notification");
                return;
            }

            // Create notifications for admins
            foreach (var adminId in adminUsers)
            {
                var command = new CreateNotificationCommand
                {
                    UserId = adminId,
                    Title = NotificationMessages.RDProject.FacultySpecialistRespondedTitle(request.ResponseText),
                    Message = NotificationMessages.RDProject.FacultySpecialistRespondedMessage(request.FacultySpecialistName, project.ProjectTitle, request.ResponseText),
                    Type = NotificationType.RDProjectFacultySpecialistResponded,
                    Link = $"/rd-projects/{project.Id}",
                    Priority = "medium",
                    ReferenceId = request.InviteId,
                    ReferenceType = "RDProjectInvite"
                };
                
                await _mediator.Send(command, cancellationToken);
            }

            _logger.LogInformation("Successfully created {Count} notifications for R&D project facultySpecialist response to invite {InviteId}", 
                adminUsers.Count, request.InviteId);
        }
    }
}
