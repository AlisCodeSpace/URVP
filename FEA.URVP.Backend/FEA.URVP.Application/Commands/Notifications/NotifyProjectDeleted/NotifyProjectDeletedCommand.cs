using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyProjectDeleted;

public sealed record NotifyProjectDeletedCommand(
    Guid ProjectId,
    Guid OwnerUserId,
    string ProjectTitle) : IRequest<int>;

public sealed class NotifyProjectDeletedCommandHandler : IRequestHandler<NotifyProjectDeletedCommand, int>
{
    public const string ReferenceType = "Project";

    private readonly IMediator _mediator;
    private readonly ILogger<NotifyProjectDeletedCommandHandler> _logger;

    public NotifyProjectDeletedCommandHandler(
        IMediator mediator,
        ILogger<NotifyProjectDeletedCommandHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public Task<int> Handle(NotifyProjectDeletedCommand request, CancellationToken cancellationToken) =>
        NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            [request.OwnerUserId],
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Projects.ProjectDeletedTitle(),
                NotificationMessages.Projects.ProjectDeletedMessage(request.ProjectTitle),
                NotificationType.ProjectDeleted,
                NotificationLinks.FacultyProjects(userId),
                NotificationPriority.Critical,
                request.ProjectId,
                ReferenceType),
            cancellationToken);
}
