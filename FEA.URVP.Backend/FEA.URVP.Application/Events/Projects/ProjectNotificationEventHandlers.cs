using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Commands.Notifications.NotifyProjectClosed;
using FEA.URVP.Application.Commands.Notifications.NotifyProjectDeleted;
using FEA.URVP.Application.Commands.Notifications.NotifyProjectOpen;
using FEA.URVP.Domain.Events.Projects;
using MediatR;

namespace FEA.URVP.Application.Events.Projects;

public sealed class ProjectOpenedEventHandler : IEventHandler<ProjectOpenedEvent>
{
    private readonly IMediator _mediator;

    public ProjectOpenedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(ProjectOpenedEvent domainEvent, CancellationToken cancellationToken = default) =>
        _mediator.Send(new NotifyProjectOpenCommand(domainEvent.ProjectId), cancellationToken);
}

public sealed class ProjectClosedEventHandler : IEventHandler<ProjectClosedEvent>
{
    private readonly IMediator _mediator;

    public ProjectClosedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(ProjectClosedEvent domainEvent, CancellationToken cancellationToken = default) =>
        _mediator.Send(
            new NotifyProjectClosedCommand(domainEvent.ProjectId, domainEvent.OwnerUserId, domainEvent.NotifyOwner),
            cancellationToken);
}

public sealed class ProjectDeletedEventHandler : IEventHandler<ProjectDeletedEvent>
{
    private readonly IMediator _mediator;

    public ProjectDeletedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(ProjectDeletedEvent domainEvent, CancellationToken cancellationToken = default) =>
        _mediator.Send(
            new NotifyProjectDeletedCommand(
                domainEvent.ProjectId,
                domainEvent.OwnerUserId,
                domainEvent.ProjectTitle),
            cancellationToken);
}
