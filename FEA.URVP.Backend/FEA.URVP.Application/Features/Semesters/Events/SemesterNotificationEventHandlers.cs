using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Commands.Notifications.NotifyApplicationWindowClosed;
using FEA.URVP.Application.Commands.Notifications.NotifyApplicationWindowOpened;
using FEA.URVP.Application.Commands.Notifications.NotifySemesterCycleStarted;
using FEA.URVP.Domain.Events.Semesters;
using MediatR;

namespace FEA.URVP.Application.Events.Semesters;

public sealed class ApplicationWindowOpenedEventHandler : IEventHandler<ApplicationWindowOpenedEvent>
{
    private readonly IMediator _mediator;

    public ApplicationWindowOpenedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(
        ApplicationWindowOpenedEvent domainEvent,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(new NotifyApplicationWindowOpenedCommand(domainEvent.SemesterId), cancellationToken);
}

public sealed class ApplicationWindowClosedEventHandler : IEventHandler<ApplicationWindowClosedEvent>
{
    private readonly IMediator _mediator;

    public ApplicationWindowClosedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(
        ApplicationWindowClosedEvent domainEvent,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(new NotifyApplicationWindowClosedCommand(domainEvent.SemesterId), cancellationToken);
}

public sealed class SemesterCycleStartedEventHandler : IEventHandler<SemesterCycleStartedEvent>
{
    private readonly IMediator _mediator;

    public SemesterCycleStartedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(
        SemesterCycleStartedEvent domainEvent,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(new NotifySemesterCycleStartedCommand(domainEvent.SemesterId), cancellationToken);
}
