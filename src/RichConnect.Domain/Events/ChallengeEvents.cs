using RICHConnect.Backend.Domain.Enums;
namespace RICHConnect.Backend.Domain.Events
{
    /// <summary>
    /// Domain event raised when a challenge is submitted by a community partner
    /// </summary>
    public class ChallengeSubmittedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ChallengeSubmitted";
        
        public Guid ChallengeId { get; }
        public Guid SubmittedByUserId { get; }
        public string ChallengeTitle { get; }
        public string SubmittedByName { get; }
        public string? Description { get; }
        public Guid ResearchFieldId { get; }
        public string? ThemeName { get; }
        public decimal EstimatedCost { get; }
        public string? SupportingDocumentUrl { get; }

        public ChallengeSubmittedEvent(
            Guid challengeId, 
            Guid submittedByUserId, 
            string challengeTitle, 
            string submittedByName,
            Guid researchFieldId,
            string? description = null,
            string? themeName = null,
            decimal estimatedCost = 0,
            string? supportingDocumentUrl = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ChallengeId = challengeId;
            SubmittedByUserId = submittedByUserId;
            ChallengeTitle = challengeTitle;
            SubmittedByName = submittedByName;
            ResearchFieldId = researchFieldId;
            Description = description;
            ThemeName = themeName;
            EstimatedCost = estimatedCost;
            SupportingDocumentUrl = supportingDocumentUrl;
        }
    }

    /// <summary>
    /// Domain event raised when a challenge is approved by an admin
    /// </summary>
    public class ChallengeApprovedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ChallengeApproved";
        
        public Guid ChallengeId { get; }
        public Guid ApprovedByUserId { get; }
        public string ChallengeTitle { get; }
        public string ApprovedByName { get; }
        public Guid SubmittedByUserId { get; }
        public string SubmittedByName { get; }
        public string? ThemeName { get; }

        public ChallengeApprovedEvent(
            Guid challengeId, 
            Guid approvedByUserId, 
            string challengeTitle, 
            string approvedByName,
            Guid submittedByUserId,
            string submittedByName,
            string? themeName = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ChallengeId = challengeId;
            ApprovedByUserId = approvedByUserId;
            ChallengeTitle = challengeTitle;
            ApprovedByName = approvedByName;
            SubmittedByUserId = submittedByUserId;
            SubmittedByName = submittedByName;
            ThemeName = themeName;
        }
    }

    /// <summary>
    /// Domain event raised when a challenge is rejected by an admin
    /// </summary>
    public class ChallengeRejectedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ChallengeRejected";
        
        public Guid ChallengeId { get; }
        public Guid RejectedByUserId { get; }
        public string ChallengeTitle { get; }
        public string RejectedByName { get; }
        public string RejectionReason { get; }
        public Guid SubmittedByUserId { get; }
        public string SubmittedByName { get; }

        public ChallengeRejectedEvent(
            Guid challengeId, 
            Guid rejectedByUserId, 
            string challengeTitle, 
            string rejectedByName, 
            string rejectionReason,
            Guid submittedByUserId,
            string submittedByName)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ChallengeId = challengeId;
            RejectedByUserId = rejectedByUserId;
            ChallengeTitle = challengeTitle;
            RejectedByName = rejectedByName;
            RejectionReason = rejectionReason;
            SubmittedByUserId = submittedByUserId;
            SubmittedByName = submittedByName;
        }
    }

    /// <summary>
    /// Domain event raised when a challenge is successfully matched with faculty specialists
    /// </summary>
    public class ChallengeMatchedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ChallengeMatched";
        
        public Guid ChallengeId { get; }
        public string ChallengeTitle { get; }
        public List<Guid> MatchedFacultySpecialistIds { get; }
        public List<string> MatchedFacultySpecialistNames { get; }
        public Guid SubmittedByUserId { get; }
        public string SubmittedByName { get; }
        public string? ThemeName { get; }
        public int TotalMatchesCreated { get; }

        public ChallengeMatchedEvent(
            Guid challengeId,
            string challengeTitle,
            List<Guid> matchedFacultySpecialistIds,
            List<string> matchedFacultySpecialistNames,
            Guid submittedByUserId,
            string submittedByName,
            string? themeName = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ChallengeId = challengeId;
            ChallengeTitle = challengeTitle;
            MatchedFacultySpecialistIds = matchedFacultySpecialistIds;
            MatchedFacultySpecialistNames = matchedFacultySpecialistNames;
            SubmittedByUserId = submittedByUserId;
            SubmittedByName = submittedByName;
            ThemeName = themeName;
            TotalMatchesCreated = matchedFacultySpecialistIds.Count;
        }
    }

    /// <summary>
    /// Domain event raised when a facultySpecialist is invited to participate in a challenge
    /// </summary>
    public class FacultySpecialistInvitedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "FacultySpecialistInvited";
        
        public Guid InviteId { get; }
        public Guid ChallengeId { get; }
        public string ChallengeTitle { get; }
        public Guid FacultySpecialistUserId { get; }
        public string FacultySpecialistName { get; }
        public string? ThemeName { get; }
        public string? PartnerName { get; }
        public string? ChallengeDescription { get; }

        public FacultySpecialistInvitedEvent(
            Guid inviteId,
            Guid challengeId,
            string challengeTitle,
            Guid facultySpecialistUserId,
            string facultySpecialistName,
            string? themeName = null,
            string? partnerName = null,
            string? challengeDescription = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            InviteId = inviteId;
            ChallengeId = challengeId;
            ChallengeTitle = challengeTitle;
            FacultySpecialistUserId = facultySpecialistUserId;
            FacultySpecialistName = facultySpecialistName;
            ThemeName = themeName;
            PartnerName = partnerName;
            ChallengeDescription = challengeDescription;
        }
    }

    /// <summary>
    /// Domain event raised when a facultySpecialist responds to a challenge invitation
    /// </summary>
    public class FacultySpecialistRespondedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "FacultySpecialistResponded";
        
        public Guid InviteId { get; }
        public Guid ChallengeId { get; }
        public string ChallengeTitle { get; }
        public Guid FacultySpecialistUserId { get; }
        public string FacultySpecialistName { get; }
        public InviteStatus Response { get; }
        public string ResponseText => Response == InviteStatus.Accepted ? "Accepted" : "Rejected";

        public FacultySpecialistRespondedEvent(
            Guid inviteId,
            Guid challengeId,
            string challengeTitle,
            Guid facultySpecialistUserId,
            string facultySpecialistName,
            InviteStatus response)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            InviteId = inviteId;
            ChallengeId = challengeId;
            ChallengeTitle = challengeTitle;
            FacultySpecialistUserId = facultySpecialistUserId;
            FacultySpecialistName = facultySpecialistName;
            Response = response;
        }
    }

    /// <summary>
    /// Domain event raised when a challenge status changes
    /// </summary>
    public class ChallengeStatusChangedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ChallengeStatusChanged";
        
        public Guid ChallengeId { get; }
        public string ChallengeTitle { get; }
        public ChallengeStatus PreviousStatus { get; }
        public ChallengeStatus NewStatus { get; }
        public Guid ChangedByUserId { get; }
        public string ChangedByName { get; }
        public string? Reason { get; }

        public ChallengeStatusChangedEvent(
            Guid challengeId,
            string challengeTitle,
            ChallengeStatus previousStatus,
            ChallengeStatus newStatus,
            Guid changedByUserId,
            string changedByName,
            string? reason = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ChallengeId = challengeId;
            ChallengeTitle = challengeTitle;
            PreviousStatus = previousStatus;
            NewStatus = newStatus;
            ChangedByUserId = changedByUserId;
            ChangedByName = changedByName;
            Reason = reason;
        }
    }

    /// <summary>
    /// Domain event raised when challenge details are updated
    /// </summary>
    public class ChallengeUpdatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ChallengeUpdated";
        
        public Guid ChallengeId { get; }
        public string ChallengeTitle { get; }
        public Guid UpdatedByUserId { get; }
        public string UpdatedByName { get; }
        public List<string> ChangedFields { get; }
        public string? UpdateReason { get; }

        public ChallengeUpdatedEvent(
            Guid challengeId,
            string challengeTitle,
            Guid updatedByUserId,
            string updatedByName,
            List<string> changedFields,
            string? updateReason = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ChallengeId = challengeId;
            ChallengeTitle = challengeTitle;
            UpdatedByUserId = updatedByUserId;
            UpdatedByName = updatedByName;
            ChangedFields = changedFields;
            UpdateReason = updateReason;
        }
    }
}
