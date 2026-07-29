using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Challenge
{
    /// <summary>
    /// Enhanced Challenge DTO that includes related entity information
    /// </summary>
    public class ChallengeWithDetailsDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public Guid ResearchFieldId { get; set; }

        // Research field details
        public string ResearchFieldName { get; set; } = null!;

        // Estimated cost for the challenge
        public decimal EstimatedCost { get; set; }

        // Supporting document
        public string? SupportingDocumentUrl { get; set; }

        public Guid SubmittedBy { get; set; }

        // Submitter details
        public string SubmitterName { get; set; } = null!;

        public ChallengeStatus Status { get; set; }

        public ChallengeMatchingStatus? MatchingStatus { get; set; }

        public Guid? ApprovedBy { get; set; }

        // Approved by details
        public string? ApprovedByName { get; set; }

        // Replaced with collection of matched facultySpecialist IDs
        public List<Guid>? MatchedFacultySpecialistIds { get; set; } = new List<Guid>();
        
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? RejectionReason { get; set; }
    }
}
