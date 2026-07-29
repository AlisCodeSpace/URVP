using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Domain.Events
{
    /// <summary>
    /// Domain event raised when an R&D project is submitted
    /// </summary>
    public class RDProjectSubmittedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "RDProjectSubmitted";
        
        public Guid RDProjectId { get; }
        public Guid SubmittedByUserId { get; }
        public string ProjectTitle { get; }
        public string SubmittedByName { get; }

        public RDProjectSubmittedEvent(
            Guid rdProjectId, 
            Guid submittedByUserId, 
            string projectTitle, 
            string submittedByName)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            RDProjectId = rdProjectId;
            SubmittedByUserId = submittedByUserId;
            ProjectTitle = projectTitle;
            SubmittedByName = submittedByName;
        }
    }

    /// <summary>
    /// Domain event raised when an R&D project is approved
    /// </summary>
    public class RDProjectApprovedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "RDProjectApproved";
        
        public Guid RDProjectId { get; }
        public Guid ApprovedByUserId { get; }
        public string ProjectTitle { get; }
        public string ApprovedByName { get; }
        public Guid SubmittedByUserId { get; }
        public string SubmittedByName { get; }

        public RDProjectApprovedEvent(
            Guid rdProjectId, 
            Guid approvedByUserId, 
            string projectTitle, 
            string approvedByName,
            Guid submittedByUserId,
            string submittedByName)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            RDProjectId = rdProjectId;
            ApprovedByUserId = approvedByUserId;
            ProjectTitle = projectTitle;
            ApprovedByName = approvedByName;
            SubmittedByUserId = submittedByUserId;
            SubmittedByName = submittedByName;
        }
    }

    /// <summary>
    /// Domain event raised when an R&D project is rejected
    /// </summary>
    public class RDProjectRejectedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "RDProjectRejected";
        
        public Guid RDProjectId { get; }
        public Guid RejectedByUserId { get; }
        public string ProjectTitle { get; }
        public string RejectedByName { get; }
        public string RejectionReason { get; }
        public Guid SubmittedByUserId { get; }
        public string SubmittedByName { get; }

        public RDProjectRejectedEvent(
            Guid rdProjectId, 
            Guid rejectedByUserId, 
            string projectTitle, 
            string rejectedByName, 
            string rejectionReason,
            Guid submittedByUserId,
            string submittedByName)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            RDProjectId = rdProjectId;
            RejectedByUserId = rejectedByUserId;
            ProjectTitle = projectTitle;
            RejectedByName = rejectedByName;
            RejectionReason = rejectionReason;
            SubmittedByUserId = submittedByUserId;
            SubmittedByName = submittedByName;
        }
    }

    /// <summary>
    /// Domain event raised when an R&D project is matched
    /// </summary>
    public class RDProjectMatchedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "RDProjectMatched";
        
        public Guid RDProjectId { get; }
        public string ProjectTitle { get; }
        public List<Guid> MatchedFacultySpecialistIds { get; }
        public List<string> MatchedFacultySpecialistNames { get; }
        public Guid SubmittedByUserId { get; }
        public string SubmittedByName { get; }
        public int TotalMatchesCreated { get; }

        public RDProjectMatchedEvent(
            Guid rdProjectId,
            string projectTitle,
            List<Guid> matchedFacultySpecialistIds,
            List<string> matchedFacultySpecialistNames,
            Guid submittedByUserId,
            string submittedByName)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            RDProjectId = rdProjectId;
            ProjectTitle = projectTitle;
            MatchedFacultySpecialistIds = matchedFacultySpecialistIds;
            MatchedFacultySpecialistNames = matchedFacultySpecialistNames;
            SubmittedByUserId = submittedByUserId;
            SubmittedByName = submittedByName;
            TotalMatchesCreated = matchedFacultySpecialistIds.Count;
        }
    }

    /// <summary>
    /// Domain event raised when a facultySpecialist is invited to an R&D project
    /// </summary>
    public class RDProjectFacultySpecialistInvitedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "RDProjectFacultySpecialistInvited";
        
        public Guid InviteId { get; }
        public Guid RDProjectId { get; }
        public string ProjectTitle { get; }
        public Guid FacultySpecialistUserId { get; }
        public string FacultySpecialistName { get; }

        public RDProjectFacultySpecialistInvitedEvent(
            Guid inviteId,
            Guid rdProjectId,
            string projectTitle,
            Guid facultySpecialistUserId,
            string facultySpecialistName)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            InviteId = inviteId;
            RDProjectId = rdProjectId;
            ProjectTitle = projectTitle;
            FacultySpecialistUserId = facultySpecialistUserId;
            FacultySpecialistName = facultySpecialistName;
        }
    }

    /// <summary>
    /// Domain event raised when a facultySpecialist responds to an R&D project invitation
    /// </summary>
    public class RDProjectFacultySpecialistRespondedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "RDProjectFacultySpecialistResponded";
        
        public Guid InviteId { get; }
        public Guid RDProjectId { get; }
        public string ProjectTitle { get; }
        public Guid FacultySpecialistUserId { get; }
        public string FacultySpecialistName { get; }
        public RDProjectInviteStatus Response { get; }
        public string ResponseText => Response == RDProjectInviteStatus.Accepted ? "Accepted" : "Rejected";

        public RDProjectFacultySpecialistRespondedEvent(
            Guid inviteId,
            Guid rdProjectId,
            string projectTitle,
            Guid facultySpecialistUserId,
            string facultySpecialistName,
            RDProjectInviteStatus response)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            InviteId = inviteId;
            RDProjectId = rdProjectId;
            ProjectTitle = projectTitle;
            FacultySpecialistUserId = facultySpecialistUserId;
            FacultySpecialistName = facultySpecialistName;
            Response = response;
        }
    }

    /// <summary>
    /// Domain event raised when an edit request is submitted for an R&D project
    /// </summary>
    public class RDProjectEditRequestedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "RDProjectEditRequested";
        
        public Guid EditRequestId { get; }
        public Guid RDProjectId { get; }
        public string ProjectTitle { get; }
        public Guid RequestedBy { get; }
        public string EditReason { get; }

        public RDProjectEditRequestedEvent(
            Guid editRequestId,
            Guid rdProjectId,
            string projectTitle,
            Guid requestedBy,
            string editReason)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            EditRequestId = editRequestId;
            RDProjectId = rdProjectId;
            ProjectTitle = projectTitle;
            RequestedBy = requestedBy;
            EditReason = editReason;
        }
    }

    /// <summary>
    /// Domain event raised when an R&D project edit request is approved
    /// </summary>
    public class RDProjectEditRequestApprovedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "RDProjectEditRequestApproved";
        
        public Guid EditRequestId { get; }
        public Guid RDProjectId { get; }
        public Guid RequestedBy { get; }
        public Guid ApprovedBy { get; }
        public DateTime ApprovedAt { get; }
        public string? AdminResponse { get; }

        public RDProjectEditRequestApprovedEvent(
            Guid editRequestId,
            Guid rdProjectId,
            Guid requestedBy,
            Guid approvedBy,
            DateTime approvedAt,
            string? adminResponse)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            EditRequestId = editRequestId;
            RDProjectId = rdProjectId;
            RequestedBy = requestedBy;
            ApprovedBy = approvedBy;
            ApprovedAt = approvedAt;
            AdminResponse = adminResponse;
        }
    }

    /// <summary>
    /// Domain event raised when an R&D project edit request is rejected
    /// </summary>
    public class RDProjectEditRequestRejectedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "RDProjectEditRequestRejected";
        
        public Guid EditRequestId { get; }
        public Guid RDProjectId { get; }
        public Guid RequestedBy { get; }
        public Guid RejectedBy { get; }
        public string AdminResponse { get; }

        public RDProjectEditRequestRejectedEvent(
            Guid editRequestId,
            Guid rdProjectId,
            Guid requestedBy,
            Guid rejectedBy,
            string adminResponse)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            EditRequestId = editRequestId;
            RDProjectId = rdProjectId;
            RequestedBy = requestedBy;
            RejectedBy = rejectedBy;
            AdminResponse = adminResponse;
        }
    }
}
