using FEA.URVP.Domain.Events;

namespace FEA.URVP.Domain.Events.Workshops;

public sealed class WorkshopAnnouncedEvent : DomainEvent
{
    public WorkshopAnnouncedEvent(Guid workshopId)
    {
        WorkshopId = workshopId;
    }

    public Guid WorkshopId { get; }
}
