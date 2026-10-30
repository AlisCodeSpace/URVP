using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Features.Notifications.Commands.NotifyStudentProfileSubmitted;
using FEA.URVP.Domain.Events.StudentProfiles;
using MediatR;

namespace FEA.URVP.Application.Features.StudentProfiles.Events;

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
