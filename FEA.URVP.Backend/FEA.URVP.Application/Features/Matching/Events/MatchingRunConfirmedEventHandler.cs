using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Commands.Notifications.NotifyMatchingConfirmed;
using FEA.URVP.Domain.Events.Matching;
using MediatR;

namespace FEA.URVP.Application.Events.Matching;

public sealed class MatchingRunConfirmedEventHandler : IEventHandler<MatchingRunConfirmedEvent>
{
    private readonly IMediator _mediator;

    public MatchingRunConfirmedEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task HandleAsync(
        MatchingRunConfirmedEvent domainEvent,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(new NotifyMatchingConfirmedCommand(domainEvent.RunId), cancellationToken);
}
