using RICHConnect.Backend.Domain.Enums;
namespace RICHConnect.Backend.Domain.Events
{
    public class ResearchFieldCreatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ResearchFieldCreated";
        
        public Guid FieldId { get; }
        public string Name { get; }
        public Guid SubmittedBy { get; }
        public ApprovalStatus Status { get; }
        public bool IsActive { get; }
        
        public ResearchFieldCreatedEvent(
            Guid fieldId,
            string name,
            Guid submittedBy,
            ApprovalStatus status,
            bool isActive)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            FieldId = fieldId;
            Name = name;
            SubmittedBy = submittedBy;
            Status = status;
            IsActive = isActive;
        }
    }

    public class ResearchFieldApprovedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ResearchFieldApproved";
        
        public Guid FieldId { get; }
        public Guid ApprovedBy { get; }
        public DateTime ApprovedAt { get; }
        
        public ResearchFieldApprovedEvent(Guid fieldId, Guid approvedBy)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            FieldId = fieldId;
            ApprovedBy = approvedBy;
            ApprovedAt = DateTime.UtcNow;
        }
    }

    public class ResearchFieldRejectedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ResearchFieldRejected";
        
        public Guid FieldId { get; }
        public Guid RejectedBy { get; }
        public string RejectionReason { get; }
        
        public ResearchFieldRejectedEvent(Guid fieldId, Guid rejectedBy, string rejectionReason)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            FieldId = fieldId;
            RejectedBy = rejectedBy;
            RejectionReason = rejectionReason;
        }
    }

    public class ResearchFieldUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ResearchFieldUpdated";
        
        public Guid FieldId { get; }
        public Guid UpdatedBy { get; }
        public Dictionary<string, object> Changes { get; }
        
        public ResearchFieldUpdatedEvent(Guid fieldId, Guid updatedBy, Dictionary<string, object> changes)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            FieldId = fieldId;
            UpdatedBy = updatedBy;
            Changes = changes;
        }
    }

    public class ResearchFieldDeletedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ResearchFieldDeleted";
        
        public Guid FieldId { get; }
        public Guid DeletedBy { get; }
        
        public ResearchFieldDeletedEvent(Guid fieldId, Guid deletedBy)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            FieldId = fieldId;
            DeletedBy = deletedBy;
        }
    }
}
