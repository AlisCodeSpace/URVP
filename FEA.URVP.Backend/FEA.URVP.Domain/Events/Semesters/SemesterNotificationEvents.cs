using FEA.URVP.Domain.Events;

namespace FEA.URVP.Domain.Events.Semesters;

public sealed class ApplicationWindowOpenedEvent : DomainEvent
{
    public ApplicationWindowOpenedEvent(Guid semesterId)
    {
        SemesterId = semesterId;
    }

    public Guid SemesterId { get; }
}

public sealed class ApplicationWindowClosedEvent : DomainEvent
{
    public ApplicationWindowClosedEvent(Guid semesterId)
    {
        SemesterId = semesterId;
    }

    public Guid SemesterId { get; }
}

public sealed class SemesterCycleStartedEvent : DomainEvent
{
    public SemesterCycleStartedEvent(Guid semesterId)
    {
        SemesterId = semesterId;
    }

    public Guid SemesterId { get; }
}
