using MediatR;
using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Infrastructure.Data.Repositories.RDProjects.Interfaces;
using RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;
using RICHConnect.Backend.Application.Services.Notifications;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectApproved
{
    public class NotifyRDProjectApprovedCommandHandler : BaseCommandHandler<NotifyRDProjectApprovedCommand>
    {
        private readonly IRDProjectRepository _rdProjectRepository;
        private readonly IMediator _mediator;

        public NotifyRDProjectApprovedCommandHandler(
            IRDProjectRepository rdProjectRepository,
            IMediator mediator,
            ILogger<NotifyRDProjectApprovedCommandHandler> logger,
            AppDbContext context)
            : base(logger, context)
        {
            _rdProjectRepository = rdProjectRepository;
            _mediator = mediator;
        }

        protected override async Task HandleInternal(NotifyRDProjectApprovedCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling NotifyRDProjectApprovedCommand for R&D project {ProjectId}", request.RDProjectId);

            var project = await _rdProjectRepository.GetByIdWithIncludesAsync(request.RDProjectId);

            if (project == null)
            {
                _logger.LogWarning("R&D project {ProjectId} not found for notification", request.RDProjectId);
                return;
            }

            // Create notification for the R&D project submitter
            var command = new CreateNotificationCommand
            {
                UserId = project.SubmittedBy,
                Title = NotificationMessages.RDProject.ApprovedTitle(),
                Message = NotificationMessages.RDProject.ApprovedMessage(project.ProjectTitle),
                Type = NotificationType.RDProjectApproved,
                Link = $"/rd-projects/{project.Id}",
                Priority = "medium",
                ReferenceId = project.Id,
                ReferenceType = "RDProject"
            };
            
            await _mediator.Send(command, cancellationToken);

            _logger.LogInformation("Successfully created notification for R&D project approval {ProjectId}", request.RDProjectId);
        }
    }
}
