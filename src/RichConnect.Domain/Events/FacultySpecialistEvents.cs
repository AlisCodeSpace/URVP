using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Domain.Events
{
    /// <summary>
    /// Domain event raised when a faculty specialist's status is updated
    /// </summary>
    public class FacultySpecialistStatusUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "FacultySpecialistStatusUpdated";
        
        public Guid FacultySpecialistUserId { get; }
        public FacultySpecialistStatus OldStatus { get; }
        public FacultySpecialistStatus NewStatus { get; }
        public string? FacultySpecialistName { get; }
        
        public FacultySpecialistStatusUpdatedEvent(
            Guid facultySpecialistUserId,
            FacultySpecialistStatus oldStatus,
            FacultySpecialistStatus newStatus,
            string? facultySpecialistName = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            FacultySpecialistUserId = facultySpecialistUserId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            FacultySpecialistName = facultySpecialistName;
        }
    }
}
