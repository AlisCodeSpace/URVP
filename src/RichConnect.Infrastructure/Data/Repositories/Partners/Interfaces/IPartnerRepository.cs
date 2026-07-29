using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Partners;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces
{
    /// <summary>
    /// Repository interface for CommunityPartner operations
    /// </summary>
    public interface IPartnerRepository
    {
        // Read operations
        Task<CommunityPartner?> GetByIdAsync(Guid id);
        Task<CommunityPartner?> GetByUserIdAsync(Guid userId);
        Task<List<CommunityPartner>> GetByStatusAsync(ApprovalStatus status);
        Task<List<CommunityPartner>> GetAllAsync();
        Task<bool> ExistsForUserAsync(Guid userId);
        
        // Write operations
        Task<CommunityPartner> AddAsync(CommunityPartner partner);
        Task UpdateAsync(CommunityPartner partner);
        Task DeleteAsync(Guid id);
        
        // Specialized queries
        Task<List<CommunityPartner>> GetPendingPartnersAsync();
        Task<int> GetCountByStatusAsync(ApprovalStatus status);
        Task<CommunityPartner?> GetPartnerWithUserAsync(Guid partnerId);
    }
}