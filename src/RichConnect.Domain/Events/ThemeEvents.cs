using RICHConnect.Backend.Domain.Enums;
namespace RICHConnect.Backend.Domain.Events
{
    /// <summary>
    /// Domain event raised when a research theme is submitted by a faculty specialist
    /// </summary>
    public class ThemeSubmittedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ThemeSubmitted";
        
        public Guid ThemeId { get; }
        public Guid SubmittedByUserId { get; }
        public string ThemeTitle { get; }
        public string SubmittedByName { get; }
        public string? Description { get; }
        public string? ExpectedOutcomes { get; }
        public double EstimatedFunding { get; }
        public Guid? ResearchFieldId { get; }
        public string? ResearchFieldName { get; }
        public string? DocumentUrl { get; }
        public string? Slug { get; }

        public ThemeSubmittedEvent(
            Guid themeId,
            Guid submittedByUserId,
            string themeTitle,
            string submittedByName,
            string? description = null,
            string? expectedOutcomes = null,
            double estimatedFunding = 0,
            Guid? researchFieldId = null,
            string? researchFieldName = null,
            string? documentUrl = null,
            string? slug = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ThemeId = themeId;
            SubmittedByUserId = submittedByUserId;
            ThemeTitle = themeTitle;
            SubmittedByName = submittedByName;
            Description = description;
            ExpectedOutcomes = expectedOutcomes;
            EstimatedFunding = estimatedFunding;
            ResearchFieldId = researchFieldId;
            ResearchFieldName = researchFieldName;
            DocumentUrl = documentUrl;
            Slug = slug;
        }
    }

    /// <summary>
    /// Domain event raised when a research theme is approved by an admin
    /// </summary>
    public class ThemeApprovedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ThemeApproved";
        
        public Guid ThemeId { get; }
        public Guid ApprovedByUserId { get; }
        public string ThemeTitle { get; }
        public string ApprovedByName { get; }
        public Guid SubmittedByUserId { get; }
        public string SubmittedByName { get; }
        public string? Description { get; }
        public string? ExpectedOutcomes { get; }
        public double EstimatedFunding { get; }
        public Guid? ResearchFieldId { get; }
        public string? ResearchFieldName { get; }
        public string? ImageUrl { get; }
        public string? DocumentUrl { get; }
        public string? Slug { get; }

        public ThemeApprovedEvent(
            Guid themeId,
            Guid approvedByUserId,
            string themeTitle,
            string approvedByName,
            Guid submittedByUserId,
            string submittedByName,
            string? description = null,
            string? expectedOutcomes = null,
            double estimatedFunding = 0,
            Guid? researchFieldId = null,
            string? researchFieldName = null,
            string? imageUrl = null,
            string? documentUrl = null,
            string? slug = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ThemeId = themeId;
            ApprovedByUserId = approvedByUserId;
            ThemeTitle = themeTitle;
            ApprovedByName = approvedByName;
            SubmittedByUserId = submittedByUserId;
            SubmittedByName = submittedByName;
            Description = description;
            ExpectedOutcomes = expectedOutcomes;
            EstimatedFunding = estimatedFunding;
            ResearchFieldId = researchFieldId;
            ResearchFieldName = researchFieldName;
            ImageUrl = imageUrl;
            DocumentUrl = documentUrl;
            Slug = slug;
        }
    }

    /// <summary>
    /// Domain event raised when a research theme is rejected by an admin
    /// </summary>
    public class ThemeRejectedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ThemeRejected";
        
        public Guid ThemeId { get; }
        public Guid RejectedByUserId { get; }
        public string ThemeTitle { get; }
        public string RejectedByName { get; }
        public Guid SubmittedByUserId { get; }
        public string SubmittedByName { get; }
        public string RejectionReason { get; }
        public string? Description { get; }
        public string? ExpectedOutcomes { get; }
        public double EstimatedFunding { get; }
        public Guid? ResearchFieldId { get; }
        public string? ResearchFieldName { get; }
        public string? DocumentUrl { get; }
        public string? Slug { get; }

        public ThemeRejectedEvent(
            Guid themeId,
            Guid rejectedByUserId,
            string themeTitle,
            string rejectedByName,
            Guid submittedByUserId,
            string submittedByName,
            string rejectionReason,
            string? description = null,
            string? expectedOutcomes = null,
            double estimatedFunding = 0,
            Guid? researchFieldId = null,
            string? researchFieldName = null,
            string? documentUrl = null,
            string? slug = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ThemeId = themeId;
            RejectedByUserId = rejectedByUserId;
            ThemeTitle = themeTitle;
            RejectedByName = rejectedByName;
            SubmittedByUserId = submittedByUserId;
            SubmittedByName = submittedByName;
            RejectionReason = rejectionReason;
            Description = description;
            ExpectedOutcomes = expectedOutcomes;
            EstimatedFunding = estimatedFunding;
            ResearchFieldId = researchFieldId;
            ResearchFieldName = researchFieldName;
            DocumentUrl = documentUrl;
            Slug = slug;
        }
    }

    /// <summary>
    /// Domain event raised when a research theme is updated by an admin
    /// </summary>
    public class ThemeUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ThemeUpdated";
        
        public Guid ThemeId { get; }
        public Guid UpdatedByUserId { get; }
        public string ThemeTitle { get; }
        public string UpdatedByName { get; }
        public string? Description { get; }
        public string? ExpectedOutcomes { get; }
        public double EstimatedFunding { get; }
        public Guid? ResearchFieldId { get; }
        public string? ResearchFieldName { get; }
        public string? ImageUrl { get; }
        public string? DocumentUrl { get; }
        public string? Slug { get; }
        public ApprovalStatus Status { get; }
        public Guid? SubmittedByUserId { get; }
        public string? SubmittedByName { get; }
        public Guid? ApprovedByUserId { get; }
        public string? ApprovedByName { get; }
        
        // Fields that were changed (for audit purposes)
        public List<string> ChangedFields { get; }
        public Dictionary<string, object> OldValues { get; }
        public Dictionary<string, object> NewValues { get; }

        public ThemeUpdatedEvent(
            Guid themeId,
            Guid updatedByUserId,
            string themeTitle,
            string updatedByName,
            string? description = null,
            string? expectedOutcomes = null,
            double estimatedFunding = 0,
            Guid? researchFieldId = null,
            string? researchFieldName = null,
            string? imageUrl = null,
            string? documentUrl = null,
            string? slug = null,
            ApprovalStatus status = ApprovalStatus.Pending,
            Guid? submittedByUserId = null,
            string? submittedByName = null,
            Guid? approvedByUserId = null,
            string? approvedByName = null,
            List<string>? changedFields = null,
            Dictionary<string, object>? oldValues = null,
            Dictionary<string, object>? newValues = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ThemeId = themeId;
            UpdatedByUserId = updatedByUserId;
            ThemeTitle = themeTitle;
            UpdatedByName = updatedByName;
            Description = description;
            ExpectedOutcomes = expectedOutcomes;
            EstimatedFunding = estimatedFunding;
            ResearchFieldId = researchFieldId;
            ResearchFieldName = researchFieldName;
            ImageUrl = imageUrl;
            DocumentUrl = documentUrl;
            Slug = slug;
            Status = status;
            SubmittedByUserId = submittedByUserId;
            SubmittedByName = submittedByName;
            ApprovedByUserId = approvedByUserId;
            ApprovedByName = approvedByName;
            ChangedFields = changedFields ?? new List<string>();
            OldValues = oldValues ?? new Dictionary<string, object>();
            NewValues = newValues ?? new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Domain event raised when a research theme is deleted by an admin
    /// </summary>
    public class ThemeDeletedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ThemeDeleted";
        
        public Guid ThemeId { get; }
        public Guid DeletedByUserId { get; }
        public string ThemeTitle { get; }
        public string DeletedByName { get; }
        public string? Description { get; }
        public string? ExpectedOutcomes { get; }
        public double EstimatedFunding { get; }
        public Guid? ResearchFieldId { get; }
        public string? ResearchFieldName { get; }
        public string? ImageUrl { get; }
        public string? DocumentUrl { get; }
        public string? Slug { get; }
        public ApprovalStatus Status { get; }
        public Guid? SubmittedByUserId { get; }
        public string? SubmittedByName { get; }
        public Guid? ApprovedByUserId { get; }
        public string? ApprovedByName { get; }
        public DateTime CreatedAt { get; }
        public DateTime UpdatedAt { get; }

        public ThemeDeletedEvent(
            Guid themeId,
            Guid deletedByUserId,
            string themeTitle,
            string deletedByName,
            string? description = null,
            string? expectedOutcomes = null,
            double estimatedFunding = 0,
            Guid? researchFieldId = null,
            string? researchFieldName = null,
            string? imageUrl = null,
            string? documentUrl = null,
            string? slug = null,
            ApprovalStatus status = ApprovalStatus.Pending,
            Guid? submittedByUserId = null,
            string? submittedByName = null,
            Guid? approvedByUserId = null,
            string? approvedByName = null,
            DateTime createdAt = default,
            DateTime updatedAt = default)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ThemeId = themeId;
            DeletedByUserId = deletedByUserId;
            ThemeTitle = themeTitle;
            DeletedByName = deletedByName;
            Description = description;
            ExpectedOutcomes = expectedOutcomes;
            EstimatedFunding = estimatedFunding;
            ResearchFieldId = researchFieldId;
            ResearchFieldName = researchFieldName;
            ImageUrl = imageUrl;
            DocumentUrl = documentUrl;
            Slug = slug;
            Status = status;
            SubmittedByUserId = submittedByUserId;
            SubmittedByName = submittedByName;
            ApprovedByUserId = approvedByUserId;
            ApprovedByName = approvedByName;
            CreatedAt = createdAt == default ? DateTime.UtcNow : createdAt;
            UpdatedAt = updatedAt == default ? DateTime.UtcNow : updatedAt;
        }
    }
}
