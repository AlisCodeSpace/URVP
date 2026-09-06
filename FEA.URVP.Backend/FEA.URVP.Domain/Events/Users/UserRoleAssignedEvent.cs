using FEA.URVP.Domain.Events;

namespace FEA.URVP.Domain.Events.Users;

public sealed class UserRoleAssignedEvent : DomainEvent
{
    public UserRoleAssignedEvent(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }
}
