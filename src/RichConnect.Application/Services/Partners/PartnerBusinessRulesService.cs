using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Partners;

namespace RICHConnect.Backend.Application.Services.Partners
{
    /// <summary>
    /// Service for partner-specific business rules and validations
    /// </summary>
    public class PartnerBusinessRulesService
    {
        private readonly IPartnerRepository _partnerRepository;
        private readonly AppDbContext _context;

        public PartnerBusinessRulesService(
            IPartnerRepository partnerRepository,
            AppDbContext context)
        {
            _partnerRepository = partnerRepository;
            _context = context;
        }

        /// <summary>
        /// Validates if a user can register as a partner
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> CanUserRegisterAsPartnerAsync(Guid userId)
        {
            // Check if user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User not found.");
            }

            // Check if user has the CommunityPartner role
            if (user.Role != UserRole.CommunityPartner)
            {
                return (false, "User does not have the CommunityPartner role.");
            }

            // Check if user already has a partner profile
            var existingPartner = await _partnerRepository.ExistsForUserAsync(userId);
            if (existingPartner)
            {
                return (false, "User already has a partner profile.");
            }

            return (true, null);
        }

        /// <summary>
        /// Validates if a partner can be approved
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> CanPartnerBeApprovedAsync(Guid partnerId)
        {
            var partner = await _partnerRepository.GetByIdAsync(partnerId);
            if (partner == null)
            {
                return (false, "Partner not found.");
            }

            if (partner.Status != ApprovalStatus.Pending)
            {
                return (false, "Only pending partners can be approved.");
            }

            // Additional business rules can be added here
            // For example: Check if all required fields are filled
            if (string.IsNullOrWhiteSpace(partner.InstitutionName))
            {
                return (false, "Institution name is required for approval.");
            }

            return (true, null);
        }

        /// <summary>
        /// Validates if a partner can be rejected
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> CanPartnerBeRejectedAsync(Guid partnerId)
        {
            var partner = await _partnerRepository.GetByIdAsync(partnerId);
            if (partner == null)
            {
                return (false, "Partner not found.");
            }

            if (partner.Status != ApprovalStatus.Pending)
            {
                return (false, "Only pending partners can be rejected.");
            }

            return (true, null);
        }

        /// <summary>
        /// Validates if a partner can update their profile
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> CanPartnerUpdateProfileAsync(Guid partnerId)
        {
            var partner = await _partnerRepository.GetByIdAsync(partnerId);
            if (partner == null)
            {
                return (false, "Partner profile not found.");
            }

            // Partners can update their profile regardless of status
            // But you might want to add restrictions here
            // For example: Rejected partners might need to re-register

            return (true, null);
        }

        /// <summary>
        /// Validates institution data for uniqueness and correctness
        /// </summary>
        public async Task<(bool IsValid, string? ErrorMessage)> ValidateInstitutionDataAsync(
            string institutionName, 
            string? chamberNumber)
        {
            // Check for duplicate institution name
            var existingPartner = await _context.CommunityPartners
                .FirstOrDefaultAsync(p => 
                    p.InstitutionName.ToLower() == institutionName.ToLower() &&
                    p.Status != ApprovalStatus.Rejected);

            if (existingPartner != null)
            {
                return (false, $"An institution with the name '{institutionName}' already exists.");
            }

            // Check for duplicate chamber of commerce number if provided
            if (!string.IsNullOrWhiteSpace(chamberNumber))
            {
                var existingByChamber = await _context.CommunityPartners
                    .FirstOrDefaultAsync(p => 
                        p.ChamberOfCommerceNumber == chamberNumber &&
                        p.Status != ApprovalStatus.Rejected);

                if (existingByChamber != null)
                {
                    return (false, $"A partner with chamber of commerce number '{chamberNumber}' already exists.");
                }
            }

            return (true, null);
        }

        /// <summary>
        /// Checks for duplicate institution name (excluding a specific partner ID for updates)
        /// </summary>
        public async Task<bool> CheckDuplicateInstitutionAsync(string institutionName, Guid? excludePartnerId = null)
        {
            var query = _context.CommunityPartners
                .Where(p => 
                    p.InstitutionName.ToLower() == institutionName.ToLower() &&
                    p.Status != ApprovalStatus.Rejected);

            if (excludePartnerId.HasValue)
            {
                query = query.Where(p => p.Id != excludePartnerId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Validates that critical fields are not empty for approval
        /// </summary>
        public (bool IsValid, string? ErrorMessage) ValidateCriticalFieldsForApproval(CommunityPartner partner)
        {
            if (string.IsNullOrWhiteSpace(partner.InstitutionName))
            {
                return (false, "Institution name is required.");
            }

            if (string.IsNullOrWhiteSpace(partner.InstitutionAddress))
            {
                return (false, "Institution address is required for approval.");
            }

            if (string.IsNullOrWhiteSpace(partner.PhoneNumber))
            {
                return (false, "Phone number is required for approval.");
            }

            if (!partner.Sector.HasValue)
            {
                return (false, "Sector is required for approval.");
            }

            return (true, null);
        }

        /// <summary>
        /// Checks if a partner has made any critical updates that require admin review
        /// </summary>
        public bool HasCriticalFieldChanges(CommunityPartner original, CommunityPartner updated)
        {
            return original.InstitutionName != updated.InstitutionName ||
                   original.ChamberOfCommerceNumber != updated.ChamberOfCommerceNumber ||
                   original.Sector != updated.Sector ||
                   original.AccreditationType != updated.AccreditationType;
        }
    }
}