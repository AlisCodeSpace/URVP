namespace RICHConnect.Backend.Domain.Events
{
    public class PartnerRegisteredEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "PartnerRegistered";
        
        public Guid PartnerId { get; }
        public Guid UserId { get; }
        public string InstitutionName { get; }
        public string Email { get; }
        
        public PartnerRegisteredEvent(Guid partnerId, Guid userId, string institutionName, string email)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            PartnerId = partnerId;
            UserId = userId;
            InstitutionName = institutionName;
            Email = email;
        }
    }

    public class PartnerApprovedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "PartnerApproved";
        
        public Guid PartnerId { get; }
        public Guid ApprovedByAdminId { get; }
        public DateTime ApprovedAt { get; }
        public string InstitutionName { get; }
        public string PartnerEmail { get; }
        
        public PartnerApprovedEvent(Guid partnerId, Guid approvedByAdminId, DateTime approvedAt, string institutionName, string partnerEmail)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            PartnerId = partnerId;
            ApprovedByAdminId = approvedByAdminId;
            ApprovedAt = approvedAt;
            InstitutionName = institutionName;
            PartnerEmail = partnerEmail;
        }
    }

    public class PartnerRejectedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "PartnerRejected";
        
        public Guid PartnerId { get; }
        public Guid RejectedByAdminId { get; }
        public string RejectionReason { get; }
        public DateTime RejectedAt { get; }
        public string InstitutionName { get; }
        public string PartnerEmail { get; }
        
        public PartnerRejectedEvent(Guid partnerId, Guid rejectedByAdminId, string rejectionReason, DateTime rejectedAt, string institutionName, string partnerEmail)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            PartnerId = partnerId;
            RejectedByAdminId = rejectedByAdminId;
            RejectionReason = rejectionReason;
            RejectedAt = rejectedAt;
            InstitutionName = institutionName;
            PartnerEmail = partnerEmail;
        }
    }

    public class PartnerUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "PartnerUpdated";
        
        public Guid PartnerId { get; }
        public Guid UpdatedByUserId { get; }
        public Dictionary<string, object> ChangedFields { get; }
        
        public PartnerUpdatedEvent(Guid partnerId, Guid updatedByUserId, Dictionary<string, object> changedFields)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            PartnerId = partnerId;
            UpdatedByUserId = updatedByUserId;
            ChangedFields = changedFields;
        }
    }
}
