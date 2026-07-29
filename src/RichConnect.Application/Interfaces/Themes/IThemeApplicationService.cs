using RICHConnect.Backend.Application.Commands.Themes.SubmitTheme;
using RICHConnect.Backend.Application.Commands.Themes.ApproveTheme;
using RICHConnect.Backend.Application.Commands.Themes.RejectTheme;
using RICHConnect.Backend.Application.Commands.Themes.UpdateTheme;
using RICHConnect.Backend.Application.Commands.Themes.DeleteTheme;
using RICHConnect.Backend.Application.Commands.Themes.PublishTheme;
using RICHConnect.Backend.Application.Commands.Themes.UnpublishTheme;
using RICHConnect.Backend.Application.Queries.Themes.GetThemeById;
using RICHConnect.Backend.Application.Queries.Themes.GetThemeBySlug;
using RICHConnect.Backend.Application.Queries.Themes.GetThemesByStatus;
using RICHConnect.Backend.Application.Queries.Themes.GetUserThemes;
using RICHConnect.Backend.Application.Queries.Themes.GetAllThemes;
using RICHConnect.Backend.Application.DTOs.Themes;

namespace RICHConnect.Backend.Application.Interfaces.Themes
{
    /// <summary>
    /// Application service interface for theme operations
    /// </summary>
    public interface IThemeApplicationService
    {
        #region Commands (Write Operations)

        /// <summary>
        /// Submit a new theme for review
        /// </summary>
        Task<ResearchThemeDto> SubmitThemeAsync(SubmitThemeCommand command);

        /// <summary>
        /// Approve a pending theme
        /// </summary>
        Task<ResearchThemeDto> ApproveThemeAsync(ApproveThemeCommand command);

        /// <summary>
        /// Reject a pending theme with reason
        /// </summary>
        Task<ResearchThemeDto> RejectThemeAsync(RejectThemeCommand command);

        /// <summary>
        /// Update an existing theme
        /// </summary>
        Task<ResearchThemeDto> UpdateThemeAsync(UpdateThemeCommand command);

        /// <summary>
        /// Delete a theme permanently
        /// </summary>
        Task<bool> DeleteThemeAsync(DeleteThemeCommand command);

        /// <summary>
        /// Publish an approved theme to make it visible on the public themes page
        /// </summary>
        Task<ResearchThemeDto> PublishThemeAsync(PublishThemeCommand command);

        /// <summary>
        /// Unpublish a theme to hide it from the public themes page
        /// </summary>
        Task<ResearchThemeDto> UnpublishThemeAsync(UnpublishThemeCommand command);

        #endregion

        #region Queries (Read Operations)

        /// <summary>
        /// Get a theme by ID
        /// </summary>
        Task<ResearchThemeDto?> GetThemeByIdAsync(GetThemeByIdQuery query);

        /// <summary>
        /// Get a theme by slug
        /// </summary>
        Task<ResearchThemeDto?> GetThemeBySlugAsync(GetThemeBySlugQuery query);

        /// <summary>
        /// Get themes by status
        /// </summary>
        Task<List<ResearchThemeDto>> GetThemesByStatusAsync(GetThemesByStatusQuery query);

        /// <summary>
        /// Get themes for a specific user
        /// </summary>
        Task<List<ResearchThemeDto>> GetUserThemesAsync(GetUserThemesQuery query);

        /// <summary>
        /// Get all themes with optional filtering
        /// </summary>
        Task<List<ResearchThemeDto>> GetAllThemesAsync(GetAllThemesQuery query);

        #endregion

        #region Business Rules and Validation

        /// <summary>
        /// Validate if a user can submit a theme
        /// </summary>
        Task<bool> CanUserSubmitThemeAsync(Guid userId);

        /// <summary>
        /// Validate if a user can approve/reject themes
        /// </summary>
        Task<bool> CanUserApproveThemesAsync(Guid userId);

        /// <summary>
        /// Validate if a user can update a specific theme
        /// </summary>
        Task<bool> CanUserUpdateThemeAsync(Guid themeId, Guid userId);

        /// <summary>
        /// Validate if a user can delete a specific theme
        /// </summary>
        Task<bool> CanUserDeleteThemeAsync(Guid themeId, Guid userId);

        /// <summary>
        /// Get theme statistics for admin dashboard
        /// </summary>
        Task<ThemeStatisticsDto> GetThemeStatisticsAsync();

        #endregion

        #region Utility Methods

        /// <summary>
        /// Generate a unique slug for a theme title
        /// </summary>
        Task<string> GenerateUniqueSlugAsync(string title, Guid? excludeThemeId = null);

        /// <summary>
        /// Check if a slug is available
        /// </summary>
        Task<bool> IsSlugAvailableAsync(string slug, Guid? excludeThemeId = null);

        /// <summary>
        /// Get themes that need admin review
        /// </summary>
        Task<List<ResearchThemeDto>> GetThemesForReviewAsync();

        #endregion
    }
}
