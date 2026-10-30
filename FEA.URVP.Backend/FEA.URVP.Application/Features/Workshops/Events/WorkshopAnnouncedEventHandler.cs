using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Commands.Notifications.NotifyWorkshopAnnounced;
using FEA.URVP.Domain.Events.Workshops;
using MediatR;

namespace FEA.URVP.Application.Events.Workshops;

public sealed class WorkshopAnnouncedEventHandler : IEventHandler<WorkshopAnnouncedEvent>
{
    private readonly IMediator _mediator;

    public WorkshopAnnouncedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(WorkshopAnnouncedEvent domainEvent, CancellationToken cancellationToken = default) =>
        _mediator.Send(new NotifyWorkshopAnnouncedCommand(domainEvent.WorkshopId), cancellationToken);
}
