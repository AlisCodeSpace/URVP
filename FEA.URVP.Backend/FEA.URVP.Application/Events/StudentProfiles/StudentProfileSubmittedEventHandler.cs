using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Commands.Notifications.NotifyStudentProfileSubmitted;
using FEA.URVP.Domain.Events.StudentProfiles;
using MediatR;

namespace FEA.URVP.Application.Events.StudentProfiles;

public sealed class StudentProfileSubmittedEventHandler : IEventHandler<StudentProfileSubmittedEvent>
{
    private readonly IMediator _mediator;

    public StudentProfileSubmittedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(
        StudentProfileSubmittedEvent domainEvent,
        CancellationToken cancellationToken = default) =>
        _mediator.Send(
            new NotifyStudentProfileSubmittedCommand(domainEvent.UserId, domainEvent.StudentName),
            cancellationToken);
}
