using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.ResearchFields;
using RICHConnect.Backend.Domain.Entities.Users;
namespace RICHConnect.Backend.Domain.Entities.Themes
{
    public class ResearchTheme
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(128)]
        public string Title { get; set; } = null!;

        [MaxLength(128)]
        public string? Slug { get; set; }

        public string? Description { get; set; }
        
        [MaxLength(2000)]
        public string? ExpectedOutcomes { get; set; }
        
        [Range(0, double.MaxValue)]
        public double EstimatedFunding { get; set; }
        
        // Phase 6: Deprecated - files now stored in FileStorage table
        [Obsolete("Use FileStorage table with EntityType='Theme' and FileCategory='Image' instead. This column will be removed in a future migration.")]
        [MaxLength(512)]
        public string? ImageUrl { get; set; }

        // Phase 6: Deprecated - files now stored in FileStorage table
        [Obsolete("Use FileStorage table with EntityType='Theme' and FileCategory='Document' instead. This column will be removed in a future migration.")]
        [MaxLength(512)]
        public string? DocumentUrl { get; set; }

        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public bool IsPublished { get; set; } = false;

        [Required]
        public Guid SubmittedBy { get; set; }
        [ForeignKey(nameof(SubmittedBy))]
        public User UserSubmitted { get; set; } = null!;

        public Guid? ApprovedBy { get; set; }
        [ForeignKey(nameof(ApprovedBy))]
        public User? UserApproved { get; set; }
        
        public Guid? ResearchFieldId { get; set; }
        [ForeignKey(nameof(ResearchFieldId))]
        public ResearchField? ResearchField { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp] public byte[] RowVersion { get; set; } = null!;
    }
}
