// File: RICHConnect.Backend/DTOs/ChallengeDto.cs

using System.ComponentModel.DataAnnotations;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Challenge
{
    public class ChallengeDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public Guid ResearchFieldId { get; set; }

        // Research field name (includes pending fields submitted by partners)
        public string? ResearchFieldName { get; set; }

        // Estimated cost for the challenge
        public decimal EstimatedCost { get; set; }

        // Supporting document
        public string? SupportingDocumentUrl { get; set; }

        public Guid SubmittedBy { get; set; }

        public ChallengeStatus Status { get; set; }

        public ChallengeMatchingStatus? MatchingStatus { get; set; }

        public Guid? ApprovedBy { get; set; }

        // Replaced with collection of matched facultySpecialist IDs
        public List<Guid>? MatchedFacultySpecialistIds { get; set; } = new List<Guid>();
        
        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string? RejectionReason { get; set; }
    }

    public class CreateMatchInvitesDto
    {
        [Required]
        public IEnumerable<Guid> FacultySpecialistUserIds { get; set; } = Array.Empty<Guid>();
    }


}
