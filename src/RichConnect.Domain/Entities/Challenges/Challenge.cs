using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.ResearchFields;
using RICHConnect.Backend.Domain.Entities.Users;
namespace RICHConnect.Backend.Domain.Entities.Challenges
{
    public class Challenge
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(128)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        public Guid ResearchFieldId { get; set; }

        [ForeignKey(nameof(ResearchFieldId))]
        public ResearchField ResearchField { get; set; } = null!;

        // Estimated cost for the challenge (using decimal for financial precision)
        [Required]
        [Range(0, 99999999999999.99)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal EstimatedCost { get; set; }

        // Supporting document (Phase 6: Deprecated - files now stored in FileStorage table)
        [Obsolete("Use FileStorage table with EntityType='Challenge' and FileCategory='SupportingDocument' instead. This column will be removed in a future migration.")]
        [MaxLength(512)]
        public string? SupportingDocumentUrl { get; set; }

        [Required]
        public Guid SubmittedBy { get; set; }

        [ForeignKey(nameof(SubmittedBy))]
        public User UserSubmitted { get; set; } = null!;

        [Required]
        public ChallengeStatus Status { get; set; } = ChallengeStatus.Pending;

        public ChallengeMatchingStatus? MatchingStatus { get; set; }

        public Guid? ApprovedBy { get; set; }

        [ForeignKey(nameof(ApprovedBy))]
        public User? UserApproved { get; set; }

        // Replaced single MatchedFacultySpecialistId with a collection of matched faculty specialists
        public ICollection<ChallengeMatchedFacultySpecialist>? MatchedFacultySpecialists { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;



        public ICollection<ChallengeMatchInvite>? MatchInvites { get; set; }
    }
}
