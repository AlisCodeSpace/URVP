using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Domain.Entities.Partners
{
    public class CommunityPartner
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required, MaxLength(128)]
        public string InstitutionName { get; set; } = null!;

        // Phase 6: Deprecated - files now stored in FileStorage table
        [Obsolete("Use FileStorage table with EntityType='Partner' and FileCategory='Logo' instead. This column will be removed in a future migration.")]
        [MaxLength(512)]
        public string? LogoUrl { get; set; }

        [MaxLength(256)]
        public string? InstitutionAddress { get; set; }

        [MaxLength(32)]
        public string? PhoneNumber { get; set; }

        [MaxLength(64)]
        public string? RegistrationNumberArea { get; set; }

        [MaxLength(64)]
        public string? ChamberOfCommerceNumber { get; set; }

        public InstitutionSector? Sector { get; set; }

        public InstitutionSize? InstitutionSize { get; set; }

        [MaxLength(2000)]
        public string? Vision { get; set; }

        [MaxLength(2000)]
        public string? Mission { get; set; }

        [MaxLength(64)]
        public string? CertificationNumber { get; set; }

        public AccreditationType? AccreditationType { get; set; }

        [Required]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 