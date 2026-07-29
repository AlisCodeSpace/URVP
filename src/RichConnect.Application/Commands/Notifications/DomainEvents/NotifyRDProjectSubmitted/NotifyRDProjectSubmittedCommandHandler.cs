using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.DomainEvents.NotifyRDProjectSubmitted
{
    public class NotifyRDProjectSubmittedCommandHandler : BaseCommandHandler<NotifyRDProjectSubmittedCommand>
    {
        private readonly IRDProjectRepository _rdProjectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;

        public NotifyRDProjectSubmittedCommandHandler(
            IRDProjectRepository rdProjectRepository,
            IUserRepository userRepository,
            IMediator mediator,
            ILogger<NotifyRDProjectSubmittedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _rdProjectRepository = rdProjectRepository;
            _userRepository = userRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyRDProjectSubmittedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyRDProjectSubmittedCommand for project {ProjectId}", request.RDProjectId);

            var project = await _rdProjectRepository.GetByIdWithIncludesAsync(request.RDProjectId);

            if (project == null)
            {
                _logger.LogWarning("RD Project {ProjectId} not found for notification", request.RDProjectId);
                return;
            }

            _logger.LogInformation("Project found: {ProjectTitle} submitted by {SubmittedBy}", 
                project.ProjectTitle, project.SubmittedBy);

            // Get all admin users
            var adminUsers = await _userRepository.GetAdminUserIdsAsync();
            _logger.LogInformation("Found {Count} admin users for notification", adminUsers.Count);

            if (!adminUsers.Any())
            {
                _logger.LogWarning("No admin users found for R&D project submission notification");
                return;
            }

            // Create notifications for admins using MediatR
            var successCount = 0;
            var failureCount = 0;
            
            foreach (var adminId in adminUsers)
            {
                try
                {
                    var command = new CreateNotificationCommand
                    {
                        UserId = adminId,
                        Title = NotificationMessages.RDProject.SubmittedTitle(),
                        Message = NotificationMessages.RDProject.SubmittedMessage(project.ProjectTitle),
                        Type = NotificationType.RDProjectSubmitted,
                        Link = $"/rd-projects/{project.Id}",
                        Priority = "high",
                        ReferenceId = project.Id,
                        ReferenceType = "RDProject"
                    };
                    
                    var notificationId = await _mediator.Send(command, cancellationToken);
                    _logger.LogInformation("Created notification {NotificationId} for admin {AdminId}", notificationId, adminId);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create notification for admin {AdminId}", adminId);
                    failureCount++;
                }
            }

            _logger.LogInformation("Successfully created {SuccessCount} notifications, {FailureCount} failures for RD project submission {ProjectId}", 
                successCount, failureCount, request.RDProjectId);
        }
    }
}
