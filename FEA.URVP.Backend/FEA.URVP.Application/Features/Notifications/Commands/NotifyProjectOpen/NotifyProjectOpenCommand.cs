using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyProjectOpen;

public sealed record NotifyProjectOpenCommand(Guid ProjectId) : IRequest<int>;

public sealed class NotifyProjectOpenCommandHandler : IRequestHandler<NotifyProjectOpenCommand, int>
{
    public const string ReferenceType = "Project";

    private readonly IProjectRepository _projects;
    private readonly ISemesterRepository _semesters;
    private readonly IUserRepository _users;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifyProjectOpenCommandHandler> _logger;

    public NotifyProjectOpenCommandHandler(
        IProjectRepository projects,
        ISemesterRepository semesters,
        IUserRepository users,
        IMediator mediator,
        ILogger<NotifyProjectOpenCommandHandler> logger)
    {
        _projects = projects;
        _semesters = semesters;
        _users = users;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(NotifyProjectOpenCommand request, CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindActiveAsync(cancellationToken);
        if (semester is null || !semester.IsApplicationWindowOpen(DateTime.UtcNow))
        {
            return 0;
        }

        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        var students = await _users.ListUserIdsByRolesAsync([UserRole.Student], cancellationToken);
        return await NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            students,
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Projects.ProjectOpenTitle(),
                NotificationMessages.Projects.ProjectOpenMessage(project.Title),
                NotificationType.ProjectOpen,
                NotificationLinks.Project(project.Id),
                NotificationPriority.Medium,
                project.Id,
                ReferenceType),
            cancellationToken);
    }
}
