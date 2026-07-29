using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Themes;
using RICHConnect.Backend.Domain.Entities.Users;
using RICHConnect.Backend.Domain.Entities.Faculty;

namespace RICHConnect.Backend.Domain.Entities.ResearchFields
{
    public class ResearchField
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(128)]
        public string Name { get; set; } = null!;

        [MaxLength(128)]
        public string? Slug { get; set; }

        [MaxLength(128)]
        public string? Category { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [Required]
        public Guid SubmittedBy { get; set; }
        [ForeignKey(nameof(SubmittedBy))]
        public User UserSubmitted { get; set; } = null!;

        public CreatorType CreatedBy { get; set; } = CreatorType.Admin;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp] 
        public byte[] RowVersion { get; set; } = null!;

        // Navigation property for themes in this field
        public ICollection<ResearchTheme>? Themes { get; set; }

        // Navigation property for faculty specialist relationships
        public ICollection<FacultySpecialistResearchField>? FacultySpecialists { get; set; }
    }
}
