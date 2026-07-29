// File: RICHConnect.Backend/DTOs/CreatePartnerDto.cs

using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Partners
{
    /// <summary>
    /// DTO for Community Partner registration
    /// 
    /// When a new partner signs up, they supply all the CommunityPartner fields.
    /// The UserId is automatically extracted from the cookie-based authentication claims.
    /// 
    /// The API will:
    ///  - Extract UserId from authentication claims (cookie-based auth)
    ///  - Create a new CommunityPartner (Status = Pending), linked to the authenticated user
    ///  - (Later) Admin can flip Status ? Approved/Rejected.
    /// </summary>
    public class CreatePartnerDto
    {
        // -------------------------------------------
        // The following fields map 1:1 to the CommunityPartner table:
        // -------------------------------------------

        /// <summary>
        /// Optional logo file. If present, the file is uploaded to FileStorage table
        /// and the file ID (GUID) is stored. Files are accessed via /api/files/{id} endpoint.
        /// </summary>
        public IFormFile? Logo { get; set; }

        [System.ComponentModel.DataAnnotations.Required]
        public string InstitutionName { get; set; } = null!;

        public string? InstitutionAddress { get; set; }

        public string? PhoneNumber { get; set; }

        public string? RegistrationNumberArea { get; set; }

        public InstitutionSector? Sector { get; set; }

        public InstitutionSize? InstitutionSize { get; set; }

        /// <summary>
        /// Changed from int ? string, because ChamberOfCommerceNumber
        /// can sometimes have leading zeros or non-numeric chars.
        /// </summary>
        public string? ChamberOfCommerceNumber { get; set; }

        public string? Vision { get; set; }

        public string? Mission { get; set; }

        public string? CertificationNumber { get; set; }

        public AccreditationType? AccreditationType { get; set; }
    }

    /// <summary>
    /// DTO for existing users to register their CommunityPartner profile
    /// 
    /// Used when a user has already been created via OAuth and now wants to
    /// register their CommunityPartner profile. No new user is created.
    /// </summary>
    public class RegisterExistingUserDto
    {
        /// <summary>
        /// Required: Company name
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string CompanyName { get; set; } = null!;

        /// <summary>
        /// Required: Full company address
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(256)]
        public string CompanyAddress { get; set; } = null!;

        /// <summary>
        /// Required: Phone number (e.g., "+961 71234567")
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(32)]
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// Required: Registration number and area (e.g., "12345 / Beirut")
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(64)]
        public string RegistrationNumberArea { get; set; } = null!;

        /// <summary>
        /// Required: One of: "Agriculture", "Manufacturing", "Technology", "Retail", "Other"
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        public string Sector { get; set; } = null!;

        /// <summary>
        /// Required: One of: "1-10", "11-50", "51-100", "101-500", "501-1000", "1000+"
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        public string CompanySize { get; set; } = null!;

        /// <summary>
        /// Required: Chamber of commerce registration number
        /// </summary>
        [System.ComponentModel.DataAnnotations.Required]
        public int ChamberOfCommerceNumber { get; set; }

        /// <summary>
        /// Optional: Company logo image file
        /// </summary>
        public IFormFile? Logo { get; set; }

        /// <summary>
        /// Optional: Company vision statement
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(2000)]
        public string? Vision { get; set; }

        /// <summary>
        /// Optional: Company mission statement
        /// </summary>
        [System.ComponentModel.DataAnnotations.MaxLength(2000)]
        public string? Mission { get; set; }
    }
}
