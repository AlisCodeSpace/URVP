using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.ResearchFields;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces
{
    public interface IResearchFieldRepository
    {
        /// <summary>
        /// Get a research field by ID
        /// </summary>
        Task<ResearchField?> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Get a research field by slug
        /// </summary>
        Task<ResearchField?> GetBySlugAsync(string slug);
        
        /// <summary>
        /// Get all active research fields
        /// </summary>
        Task<IEnumerable<ResearchField>> GetAllActiveAsync();
        
        /// <summary>
        /// Get all research fields including inactive ones
        /// </summary>
        Task<IEnumerable<ResearchField>> GetAllIncludingInactiveAsync();
        
        /// <summary>
        /// Get research fields by approval status
        /// </summary>
        Task<IEnumerable<ResearchField>> GetByStatusAsync(ApprovalStatus status);
        
        /// <summary>
        /// Get research fields submitted by a specific user
        /// </summary>
        Task<IEnumerable<ResearchField>> GetBySubmitterAsync(Guid userId);
        
        /// <summary>
        /// Get research fields available for a specific user
        /// </summary>
        Task<IEnumerable<ResearchField>> GetAvailableFieldsForUserAsync(Guid userId);
        
        /// <summary>
        /// Add a new research field
        /// </summary>
        Task<ResearchField> AddAsync(ResearchField field);
        
        /// <summary>
        /// Update an existing research field
        /// </summary>
        Task<ResearchField?> UpdateAsync(ResearchField field);
        
        /// <summary>
        /// Delete a research field by ID
        /// </summary>
        Task<bool> DeleteAsync(Guid id);
        
        /// <summary>
        /// Check if a research field exists by ID
        /// </summary>
        Task<bool> ExistsAsync(Guid id);
        
        /// <summary>
        /// Check if a research field exists by slug
        /// </summary>
        Task<bool> ExistsBySlugAsync(string slug);
        
        /// <summary>
        /// Get a research field with its submitter user information
        /// </summary>
        Task<ResearchField?> GetFieldWithUserAsync(Guid fieldId);
    }
}
