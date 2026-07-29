using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Faculty
{
    /// <summary>
    /// DTO for facultySpecialist challenge view that combines invite and challenge data
    /// </summary>
    public class FacultySpecialistChallengeDto
    {
        // Invite data
        public Guid? InviteId { get; set; }
        public InviteStatus? InviteStatus { get; set; }
        public DateTime? InviteCreatedAt { get; set; }
        public DateTime? InviteUpdatedAt { get; set; }
        
        // Challenge data
        public Guid ChallengeId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Guid ResearchFieldId { get; set; }
        public string? ResearchFieldName { get; set; }
        public decimal EstimatedCost { get; set; }
        public string? SupportingDocumentUrl { get; set; }
        public Guid SubmittedBy { get; set; }
        public string? SubmitterName { get; set; }
        public ChallengeStatus Status { get; set; }
        public ChallengeMatchingStatus? MatchingStatus { get; set; }
        public DateTime ChallengeCreatedAt { get; set; }
        public DateTime ChallengeUpdatedAt { get; set; }
        
        // Participation data (for finalized matches)
        public bool IsParticipating { get; set; }
        public DateTime? MatchedAt { get; set; }
    }
}
