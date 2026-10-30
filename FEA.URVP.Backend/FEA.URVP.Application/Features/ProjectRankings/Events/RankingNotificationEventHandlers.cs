using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Commands.Notifications.NotifyRankingRemoved;
using FEA.URVP.Application.Commands.Notifications.NotifyRankingSubmitted;
using FEA.URVP.Domain.Events.Rankings;
using MediatR;

namespace FEA.URVP.Application.Events.Rankings;

public sealed class ProjectRankingSubmittedEventHandler : IEventHandler<ProjectRankingSubmittedEvent>
{
    private readonly IMediator _mediator;

    public ProjectRankingSubmittedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(
        ProjectRankingSubmittedEvent domainEvent,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(
            new NotifyRankingSubmittedCommand(
                domainEvent.RankingId,
                domainEvent.ProjectId,
                domainEvent.OwnerUserId,
                domainEvent.ProjectTitle,
                domainEvent.StudentName),
            cancellationToken);
}

public sealed class ProjectRankingRemovedEventHandler : IEventHandler<ProjectRankingRemovedEvent>
{
    private readonly IMediator _mediator;

    public ProjectRankingRemovedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(
        ProjectRankingRemovedEvent domainEvent,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(
            new NotifyRankingRemovedCommand(
                domainEvent.ProjectId,
                domainEvent.OwnerUserId,
                domainEvent.ProjectTitle,
                domainEvent.StudentName),
            cancellationToken);
}
