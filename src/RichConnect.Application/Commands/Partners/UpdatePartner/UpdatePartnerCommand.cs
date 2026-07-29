using MediatR;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Partners.UpdatePartner
{
    /// <summary>
    /// Command for updating an existing community partner profile
    /// </summary>
    public class UpdatePartnerCommand : IRequest<CommunityPartnerDto>
    {
        /// <summary>
        /// User ID from authentication (must match the partner's UserId)
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Optional logo file for upload
        /// </summary>
        public IFormFile? Logo { get; set; }
        
        /// <summary>
        /// Optional institution name
        /// </summary>
        public string? InstitutionName { get; set; }
        
        /// <summary>
        /// Optional institution address
        /// </summary>
        public string? InstitutionAddress { get; set; }
        
        /// <summary>
        /// Optional phone number
        /// </summary>
        public string? PhoneNumber { get; set; }
        
        /// <summary>
        /// Optional registration number area
        /// </summary>
        public string? RegistrationNumberArea { get; set; }
        
        /// <summary>
        /// Optional sector
        /// </summary>
        public InstitutionSector? Sector { get; set; }
        
        /// <summary>
        /// Optional institution size
        /// </summary>
        public InstitutionSize? InstitutionSize { get; set; }
        
        /// <summary>
        /// Optional chamber of commerce number
        /// </summary>
        public string? ChamberOfCommerceNumber { get; set; }
        
        /// <summary>
        /// Optional vision statement
        /// </summary>
        public string? Vision { get; set; }
        
        /// <summary>
        /// Optional mission statement
        /// </summary>
        public string? Mission { get; set; }
        
        /// <summary>
        /// Optional certification number
        /// </summary>
        public string? CertificationNumber { get; set; }
        
        /// <summary>
        /// Optional accreditation type
        /// </summary>
        public AccreditationType? AccreditationType { get; set; }
    }
}