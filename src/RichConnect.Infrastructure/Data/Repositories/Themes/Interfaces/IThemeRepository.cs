using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces
{
    /// <summary>
    /// Repository interface for ResearchTheme operations
    /// </summary>
    public interface IThemeRepository
    {
        // Core CRUD Operations
        Task<ResearchTheme?> GetByIdAsync(Guid id);
        Task<ResearchTheme?> GetByIdWithIncludesAsync(Guid id);
        Task<ResearchTheme?> GetBySlugAsync(string slug);
        Task<ResearchTheme?> GetBySlugWithIncludesAsync(string slug);
        Task<List<ResearchTheme>> GetAllAsync();
        Task<List<ResearchTheme>> GetAllWithIncludesAsync();
        Task<ResearchTheme> CreateAsync(ResearchTheme theme);
        Task<ResearchTheme> UpdateAsync(ResearchTheme theme);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsBySlugAsync(string slug);

        // Status-based queries
        Task<List<ResearchTheme>> GetByStatusAsync(ApprovalStatus status);
        Task<List<ResearchTheme>> GetByStatusWithIncludesAsync(ApprovalStatus status);
        Task<List<ResearchTheme>> GetPendingAsync();
        Task<List<ResearchTheme>> GetPendingWithIncludesAsync();
        Task<List<ResearchTheme>> GetApprovedAsync();
        Task<List<ResearchTheme>> GetApprovedWithIncludesAsync();
        Task<List<ResearchTheme>> GetRejectedAsync();
        Task<List<ResearchTheme>> GetRejectedWithIncludesAsync();

        // User-based queries
        Task<List<ResearchTheme>> GetByUserAsync(Guid userId);
        Task<List<ResearchTheme>> GetByUserWithIncludesAsync(Guid userId);
        Task<List<ResearchTheme>> GetByApproverAsync(Guid approverId);
        Task<List<ResearchTheme>> GetByApproverWithIncludesAsync(Guid approverId);

        // Research field queries
        Task<List<ResearchTheme>> GetByResearchFieldAsync(Guid researchFieldId);
        Task<List<ResearchTheme>> GetByResearchFieldWithIncludesAsync(Guid researchFieldId);
        Task<List<ResearchTheme>> GetByResearchFieldAndStatusAsync(Guid researchFieldId, ApprovalStatus status);
        Task<List<ResearchTheme>> GetByResearchFieldAndStatusWithIncludesAsync(Guid researchFieldId, ApprovalStatus status);

        // Admin queries
        Task<List<ResearchTheme>> GetForAdminReviewAsync();
        Task<List<ResearchTheme>> GetForAdminReviewWithIncludesAsync();
        Task<int> GetCountByStatusAsync(ApprovalStatus status);
        Task<int> GetCountByUserAsync(Guid userId);
        Task<int> GetCountByResearchFieldAsync(Guid researchFieldId);

        // Search and filtering
        Task<List<ResearchTheme>> SearchByTitleAsync(string searchTerm);
        Task<List<ResearchTheme>> SearchByTitleWithIncludesAsync(string searchTerm);
        Task<List<ResearchTheme>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<ResearchTheme>> GetByDateRangeWithIncludesAsync(DateTime startDate, DateTime endDate);

        // Validation methods
        Task<bool> ValidateResearchFieldExistsAsync(Guid researchFieldId);
        Task<bool> ValidateUserExistsAsync(Guid userId);
        Task<bool> ValidateSlugIsUniqueAsync(string slug, Guid? excludeId = null);
        Task<bool> ValidateTitleIsUniqueAsync(string title, Guid? excludeId = null);
        Task<bool> HasDependenciesAsync(Guid themeId);

        // Bulk operations
        Task<List<ResearchTheme>> GetByIdsAsync(List<Guid> ids);
        Task<List<ResearchTheme>> GetByIdsWithIncludesAsync(List<Guid> ids);
        Task<int> DeleteByStatusAsync(ApprovalStatus status);
        Task<int> UpdateStatusAsync(List<Guid> ids, ApprovalStatus status, Guid updatedBy);

        // Statistics and analytics
        Task<Dictionary<ApprovalStatus, int>> GetStatusCountsAsync();
        Task<Dictionary<Guid, int>> GetCountsByResearchFieldAsync();
        Task<Dictionary<Guid, int>> GetCountsByUserAsync();
        Task<List<ResearchTheme>> GetRecentlyUpdatedAsync(int days = 7);
        Task<List<ResearchTheme>> GetRecentlyUpdatedWithIncludesAsync(int days = 7);
        
        /// <summary>
        /// Get a theme with its submitter user information
        /// </summary>
        Task<ResearchTheme?> GetThemeWithUserAsync(Guid themeId);
    }
}
