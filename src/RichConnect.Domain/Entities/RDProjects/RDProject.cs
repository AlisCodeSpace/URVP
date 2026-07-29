using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.ResearchFields;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Domain.Entities.RDProjects
{
    public class RDProject
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(200)]
        public string ProjectTitle { get; set; } = null!;

        [Required]
        public string BriefDescription { get; set; } = null!;

        [Required, MaxLength(1000)]
        public string OrganizationResources { get; set; } = null!;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string KeyDeliverables { get; set; } = null!;

        [Required]
        public string IpConfidentialityRequirements { get; set; } = null!;

        // Support types stored as child entities
        public ICollection<RDProjectSupportType> SupportTypes { get; set; } = new List<RDProjectSupportType>();

        [MaxLength(500)]
        public string? OtherSupportType { get; set; }

        // Optional research field link (if community partners specify one)
        public Guid? ResearchFieldId { get; set; }

        [ForeignKey(nameof(ResearchFieldId))]
        public ResearchField? ResearchField { get; set; }

        [Required]
        public Guid SubmittedBy { get; set; }

        [ForeignKey(nameof(SubmittedBy))]
        public User UserSubmitted { get; set; } = null!;

        [Required]
        public RDProjectStatus Status { get; set; } = RDProjectStatus.Pending;

        public RDProjectMatchingStatus? MatchingStatus { get; set; }

        public Guid? ApprovedBy { get; set; }

        [ForeignKey(nameof(ApprovedBy))]
        public User? UserApproved { get; set; }

        public ICollection<RDProjectMatchedFacultySpecialist>? MatchedFacultySpecialists { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public ICollection<RDProjectMatchInvite>? MatchInvites { get; set; }
    }
}
