using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectRejected
{
    public class NotifyRDProjectRejectedCommandHandler : BaseCommandHandler<NotifyRDProjectRejectedCommand>
    {
        private readonly IRDProjectRepository _rdProjectRepository;
        private readonly IMediator _mediator;

        public NotifyRDProjectRejectedCommandHandler(
            IRDProjectRepository rdProjectRepository,
            IMediator mediator,
            ILogger<NotifyRDProjectRejectedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _rdProjectRepository = rdProjectRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyRDProjectRejectedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyRDProjectRejectedCommand for R&D project {ProjectId}", request.RDProjectId);

            var project = await _rdProjectRepository.GetByIdWithIncludesAsync(request.RDProjectId);

            if (project == null)
            {
                _logger.LogWarning("R&D project {ProjectId} not found for notification", request.RDProjectId);
                return;
            }

            // Create notification for R&D project submitter
            var command = new CreateNotificationCommand
            {
                UserId = project.SubmittedBy,
                Title = NotificationMessages.RDProject.RejectedTitle(),
                Message = NotificationMessages.RDProject.RejectedMessage(project.ProjectTitle, request.RejectionReason),
                Type = NotificationType.RDProjectRejected,
                Link = $"/rd-projects/{project.Id}",
                Priority = "high"
            };

            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully created notification for R&D project rejection {ProjectId}", request.RDProjectId);
        }
    }
}
