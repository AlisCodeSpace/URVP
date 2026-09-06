using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyProjectClosed;

public sealed record NotifyProjectClosedCommand(
    Guid ProjectId,
    Guid OwnerUserId,
    bool NotifyOwner) : IRequest<int>;

public sealed class NotifyProjectClosedCommandHandler : IRequestHandler<NotifyProjectClosedCommand, int>
{
    public const string ReferenceType = "Project";

    private readonly IProjectRepository _projects;
    private readonly IProjectRankingRepository _rankings;
    private readonly IMatchingRunRepository _runs;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifyProjectClosedCommandHandler> _logger;

    public NotifyProjectClosedCommandHandler(
        IProjectRepository projects,
        IProjectRankingRepository rankings,
        IMatchingRunRepository runs,
        IMediator mediator,
        ILogger<NotifyProjectClosedCommandHandler> logger)
    {
        _projects = projects;
        _rankings = rankings;
        _runs = runs;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(NotifyProjectClosedCommand request, CancellationToken cancellationToken)
    {
        var project = await _projects.FindByIdAsync(request.ProjectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.ProjectId} was not found.");

        var ranked = await _rankings.ListByProjectAsync(project.Id, cancellationToken);
        var placed = await _runs.ListConfirmedByProjectAsync(project.Id, cancellationToken);
        var recipients = ranked.Select(r => r.StudentUserId)
            .Concat(placed.Select(p => p.StudentUserId))
            .ToList();

        if (request.NotifyOwner)
        {
            recipients.Add(request.OwnerUserId);
        }

        return await NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            recipients,
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Projects.ProjectClosedTitle(),
                NotificationMessages.Projects.ProjectClosedMessage(project.Title),
                NotificationType.ProjectClosed,
                userId == request.OwnerUserId
                    ? NotificationLinks.FacultyProject(request.OwnerUserId, project.Id)
                    : NotificationLinks.Project(project.Id),
                NotificationPriority.Low,
                project.Id,
                ReferenceType),
            cancellationToken);
    }
}
