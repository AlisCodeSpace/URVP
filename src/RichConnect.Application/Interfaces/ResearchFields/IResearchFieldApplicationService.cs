using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Application.Commands.ResearchFields.ApproveField;
using RICHConnect.Backend.Application.Commands.ResearchFields.CreateField;
using RICHConnect.Backend.Application.Commands.ResearchFields.DeleteField;
using RICHConnect.Backend.Application.Commands.ResearchFields.RejectField;
using RICHConnect.Backend.Application.Commands.ResearchFields.UpdateField;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Interfaces.ResearchFields
{
    /// <summary>
    /// Application service for managing research fields
    /// </summary>
    public interface IResearchFieldApplicationService
    {
        // Command methods
        
        /// <summary>
        /// Create a new research field
        /// </summary>
        Task<ResearchFieldDto> CreateFieldAsync(CreateFieldCommand command);
        
        /// <summary>
        /// Update an existing research field
        /// </summary>
        Task<ResearchFieldDto> UpdateFieldAsync(UpdateFieldCommand command);
        
        /// <summary>
        /// Approve a pending research field
        /// </summary>
        Task<bool> ApproveFieldAsync(ApproveFieldCommand command);
        
        /// <summary>
        /// Reject a pending research field
        /// </summary>
        Task<bool> RejectFieldAsync(RejectFieldCommand command);
        
        /// <summary>
        /// Delete a research field
        /// </summary>
        Task<bool> DeleteFieldAsync(DeleteFieldCommand command);
        
        // Query methods
        
        /// <summary>
        /// Get a research field by ID
        /// </summary>
        Task<ResearchFieldDto> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Get a research field by slug
        /// </summary>
        Task<ResearchFieldDto> GetBySlugAsync(string slug);
        
        /// <summary>
        /// Get all active research fields
        /// </summary>
        Task<IEnumerable<ResearchFieldDto>> GetAllActiveAsync();
        
        /// <summary>
        /// Get all research fields including inactive ones
        /// </summary>
        Task<IEnumerable<ResearchFieldDto>> GetAllIncludingInactiveAsync();
        
        /// <summary>
        /// Get research fields by approval status
        /// </summary>
        Task<IEnumerable<ResearchFieldDto>> GetByStatusAsync(ApprovalStatus status);
        
        /// <summary>
        /// Get research fields submitted by a specific user
        /// </summary>
        Task<IEnumerable<ResearchFieldDto>> GetBySubmitterAsync(Guid userId);
        
        /// <summary>
        /// Get research fields available for a specific user
        /// </summary>
        Task<IEnumerable<ResearchFieldDto>> GetAvailableFieldsForUserAsync(Guid userId);
        
        // Validation methods
        
        /// <summary>
        /// Check if a user can approve a research field
        /// </summary>
        Task<bool> CanApproveFieldAsync(Guid fieldId);
        
        /// <summary>
        /// Check if a user can reject a research field
        /// </summary>
        Task<bool> CanRejectFieldAsync(Guid fieldId);
        
        /// <summary>
        /// Check if a user can delete a research field
        /// </summary>
        Task<bool> CanDeleteFieldAsync(Guid fieldId);
        
        /// <summary>
        /// Check if a slug is unique
        /// </summary>
        Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null);
    }
}
