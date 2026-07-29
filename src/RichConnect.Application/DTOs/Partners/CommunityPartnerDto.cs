// File: RICHConnect.Backend/DTOs/CommunityPartnerDto.cs

using System.ComponentModel.DataAnnotations;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Partners
{
    public class CommunityPartnerDto
    {
        public Guid Id { get; set; }

        /// <summary>
        /// The user (owner) who originally submitted this CommunityPartner.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// The email address of the user who owns this CommunityPartner profile
        /// </summary>
        public string Email { get; set; } = null!;

        public string InstitutionName { get; set; } = null!;

        public string? LogoUrl { get; set; }

        public string? InstitutionAddress { get; set; }

        public string? PhoneNumber { get; set; }

        public string? RegistrationNumberArea { get; set; }

        public string? ChamberOfCommerceNumber { get; set; }

        public InstitutionSector? Sector { get; set; }

        public InstitutionSize? InstitutionSize { get; set; }

        public string? Vision { get; set; }

        public string? Mission { get; set; }

        public string? CertificationNumber { get; set; }

        public AccreditationType? AccreditationType { get; set; }

        /// <summary>
        /// Pending=0, Approved=1, Rejected=2
        /// </summary>
        public ApprovalStatus Status { get; set; }

        public DateTime SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for updating CommunityPartner profile information
    /// </summary>
    public class UpdateCommunityPartnerDto
    {
        public string? InstitutionName { get; set; }

        public string? InstitutionAddress { get; set; }

        public string? PhoneNumber { get; set; }

        public string? RegistrationNumberArea { get; set; }

        public string? ChamberOfCommerceNumber { get; set; }

        public InstitutionSector? Sector { get; set; }

        public InstitutionSize? InstitutionSize { get; set; }

        public string? Vision { get; set; }

        public string? Mission { get; set; }

        public string? CertificationNumber { get; set; }

        public AccreditationType? AccreditationType { get; set; }

        /// <summary>
        /// Optional logo file for upload
        /// </summary>
        public IFormFile? Logo { get; set; }
    }

    /// <summary>
    /// DTO for rejecting a CommunityPartner
    /// </summary>
    public class RejectCommunityPartnerDto
    {
        /// <summary>
        /// Required reason for rejection
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string RejectionReason { get; set; } = null!;
    }
} 