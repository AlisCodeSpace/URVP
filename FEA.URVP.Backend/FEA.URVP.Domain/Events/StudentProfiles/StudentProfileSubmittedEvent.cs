using FEA.URVP.Domain.Events;

namespace FEA.URVP.Domain.Events.StudentProfiles;

public sealed class StudentProfileSubmittedEvent : DomainEvent
{
    public StudentProfileSubmittedEvent(Guid userId, string studentName)
    {
        UserId = userId;
        StudentName = studentName;
    }

    public Guid UserId { get; }
    public string StudentName { get; }
}
