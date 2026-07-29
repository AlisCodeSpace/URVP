using RICHConnect.Backend.Application.Commands.Partners.ApprovePartner;
using RICHConnect.Backend.Application.Commands.Partners.RegisterPartner;
using RICHConnect.Backend.Application.Commands.Partners.RejectPartner;
using RICHConnect.Backend.Application.Commands.Partners.UpdatePartner;
using RICHConnect.Backend.Application.DTOs.Partners;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Interfaces.Partners
{
    /// <summary>
    /// Application service for community partner operations
    /// </summary>
    public interface IPartnerApplicationService
    {
        /// <summary>
        /// Registers a new community partner
        /// </summary>
        Task<CommunityPartnerDto> RegisterPartnerAsync(RegisterPartnerCommand command);
        
        /// <summary>
        /// Updates an existing community partner profile
        /// </summary>
        Task<CommunityPartnerDto> UpdatePartnerAsync(UpdatePartnerCommand command);
        
        /// <summary>
        /// Approves a pending community partner
        /// </summary>
        Task<bool> ApprovePartnerAsync(ApprovePartnerCommand command);
        
        /// <summary>
        /// Rejects a pending community partner
        /// </summary>
        Task<bool> RejectPartnerAsync(RejectPartnerCommand command);
        
        /// <summary>
        /// Gets a community partner by ID
        /// </summary>
        Task<CommunityPartnerDto?> GetPartnerByIdAsync(Guid partnerId);
        
        /// <summary>
        /// Gets a community partner by user ID
        /// </summary>
        Task<CommunityPartnerDto?> GetPartnerByUserIdAsync(Guid userId);
        
        /// <summary>
        /// Gets community partners by status
        /// </summary>
        Task<List<CommunityPartnerDto>> GetPartnersByStatusAsync(
            ApprovalStatus? status = null, 
            int pageNumber = 1, 
            int pageSize = 50, 
            string? sortBy = "SubmittedAt", 
            bool sortDescending = true);
    }
}