using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Commands.Notifications.NotifyPlacementCancelled;
using FEA.URVP.Application.Commands.Notifications.NotifyPlacementDeclined;
using FEA.URVP.Domain.Events.Matching;
using MediatR;

namespace FEA.URVP.Application.Events.Matching;

public sealed class PlacementDeclinedEventHandler : IEventHandler<PlacementDeclinedEvent>
{
    private readonly IMediator _mediator;

    public PlacementDeclinedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(PlacementDeclinedEvent domainEvent, CancellationToken cancellationToken = default) =>
        _mediator.Send(new NotifyPlacementDeclinedCommand(domainEvent.PlacementId), cancellationToken);
}

public sealed class PlacementCancelledEventHandler : IEventHandler<PlacementCancelledEvent>
{
    private readonly IMediator _mediator;

    public PlacementCancelledEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(PlacementCancelledEvent domainEvent, CancellationToken cancellationToken = default) =>
        _mediator.Send(new NotifyPlacementCancelledCommand(domainEvent.PlacementId), cancellationToken);
}
