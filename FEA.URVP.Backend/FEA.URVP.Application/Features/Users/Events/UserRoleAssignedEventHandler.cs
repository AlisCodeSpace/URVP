using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Commands.Notifications.NotifyRoleAssigned;
using FEA.URVP.Domain.Events.Users;
using MediatR;

namespace FEA.URVP.Application.Events.Users;

public sealed class UserRoleAssignedEventHandler : IEventHandler<UserRoleAssignedEvent>
{
    private readonly IMediator _mediator;

    public UserRoleAssignedEventHandler(IMediator mediator) => _mediator = mediator;

    public Task HandleAsync(UserRoleAssignedEvent domainEvent, CancellationToken cancellationToken = default) =>
        _mediator.Send(new NotifyRoleAssignedCommand(domainEvent.UserId), cancellationToken);
}
