using FEA.URVP.Domain.Events;

namespace FEA.URVP.Domain.Events.Matching;

public sealed class PlacementAssignedEvent : DomainEvent
{
    public PlacementAssignedEvent(Guid placementId)
    {
        PlacementId = placementId;
    }

    public Guid PlacementId { get; }
}

public sealed class PlacementDeclinedEvent : DomainEvent
{
    public PlacementDeclinedEvent(Guid placementId)
    {
        PlacementId = placementId;
    }

    public Guid PlacementId { get; }
}

public sealed class PlacementCancelledEvent : DomainEvent
{
    public PlacementCancelledEvent(Guid placementId)
    {
        PlacementId = placementId;
    }

    public Guid PlacementId { get; }
}
