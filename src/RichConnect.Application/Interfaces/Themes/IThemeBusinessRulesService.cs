using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Interfaces.Themes
{
    /// <summary>
    /// Interface for theme business rules service
    /// </summary>
    public interface IThemeBusinessRulesService
    {
        #region Theme Submission Rules

        /// <summary>
        /// Validate if a user can submit a theme
        /// </summary>
        Task<ValidationResult> CanUserSubmitThemeAsync(Guid userId);

        /// <summary>
        /// Validate theme submission data
        /// </summary>
        Task<ValidationResult> ValidateThemeSubmissionAsync(FacultySpecialistThemeSubmissionDto dto, Guid userId);

        /// <summary>
        /// Validate admin theme creation
        /// </summary>
        Task<ValidationResult> ValidateAdminThemeCreationAsync(AdminThemeCreationDto dto, Guid adminUserId);

        /// <summary>
        /// Check if user has reached theme submission limit
        /// </summary>
        Task<ValidationResult> CheckThemeSubmissionLimitAsync(Guid userId);

        #endregion

        #region Theme Approval/Rejection Rules

        /// <summary>
        /// Validate if a user can approve themes
        /// </summary>
        Task<ValidationResult> CanUserApproveThemesAsync(Guid userId);

        /// <summary>
        /// Validate theme approval
        /// </summary>
        Task<ValidationResult> ValidateThemeApprovalAsync(Guid themeId, Guid approverId);

        /// <summary>
        /// Validate theme rejection
        /// </summary>
        Task<ValidationResult> ValidateThemeRejectionAsync(Guid themeId, Guid rejectorId, string rejectionReason);

        /// <summary>
        /// Check if theme is in correct state for approval/rejection
        /// </summary>
        Task<ValidationResult> ValidateThemeStateForActionAsync(Guid themeId, string action);

        #endregion

        #region Theme Update Rules

        /// <summary>
        /// Validate if a user can update a theme
        /// </summary>
        Task<ValidationResult> CanUserUpdateThemeAsync(Guid themeId, Guid userId);

        /// <summary>
        /// Validate theme update data
        /// </summary>
        Task<ValidationResult> ValidateThemeUpdateAsync(AdminThemeUpdateDto dto, Guid themeId, Guid userId);

        /// <summary>
        /// Check if theme can be updated (not in certain states)
        /// </summary>
        Task<ValidationResult> ValidateThemeUpdateabilityAsync(Guid themeId);

        /// <summary>
        /// Validate slug uniqueness for updates
        /// </summary>
        Task<ValidationResult> ValidateSlugUniquenessAsync(string slug, Guid? excludeThemeId = null);

        #endregion

        #region Theme Deletion Rules

        /// <summary>
        /// Validate if a user can delete a theme
        /// </summary>
        Task<ValidationResult> CanUserDeleteThemeAsync(Guid themeId, Guid userId);

        /// <summary>
        /// Validate theme deletion
        /// </summary>
        Task<ValidationResult> ValidateThemeDeletionAsync(Guid themeId, Guid deleterId);

        /// <summary>
        /// Check if theme has dependencies that prevent deletion
        /// </summary>
        Task<ValidationResult> ValidateThemeDeletionDependenciesAsync(Guid themeId);

        #endregion

        #region General Business Rules

        /// <summary>
        /// Validate research field assignment
        /// </summary>
        Task<ValidationResult> ValidateResearchFieldAssignmentAsync(Guid? researchFieldId);

        /// <summary>
        /// Validate estimated funding
        /// </summary>
        Task<ValidationResult> ValidateEstimatedFundingAsync(double estimatedFunding);

        /// <summary>
        /// Validate file uploads
        /// </summary>
        Task<ValidationResult> ValidateFileUploadsAsync(IFormFile? image, IFormFile? document);

        /// <summary>
        /// Check theme title uniqueness
        /// </summary>
        Task<ValidationResult> ValidateTitleUniquenessAsync(string title, Guid? excludeThemeId = null);

        /// <summary>
        /// Validate theme content quality
        /// </summary>
        Task<ValidationResult> ValidateThemeContentQualityAsync(string title, string? description, string? expectedOutcomes);

        #endregion

        #region Role-Based Validation

        /// <summary>
        /// Check if user has admin privileges
        /// </summary>
        Task<ValidationResult> ValidateAdminPrivilegesAsync(Guid userId);

        /// <summary>
        /// Check if user has faculty specialist privileges
        /// </summary>
        Task<ValidationResult> ValidateFacultySpecialistPrivilegesAsync(Guid userId);

        /// <summary>
        /// Check if user can access theme (view permissions)
        /// </summary>
        Task<ValidationResult> CanUserAccessThemeAsync(Guid themeId, Guid userId);

        #endregion

        #region Workflow Rules

        /// <summary>
        /// Validate theme workflow transitions
        /// </summary>
        Task<ValidationResult> ValidateThemeWorkflowTransitionAsync(Guid themeId, ApprovalStatus fromStatus, ApprovalStatus toStatus, Guid userId);

        /// <summary>
        /// Check if theme can be published
        /// </summary>
        Task<ValidationResult> CanThemeBePublishedAsync(Guid themeId);

        /// <summary>
        /// Check if theme can be archived
        /// </summary>
        Task<ValidationResult> CanThemeBeArchivedAsync(Guid themeId);

        #endregion
    }

    /// <summary>
    /// Result of business rule validation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> AdditionalData { get; set; } = new();

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult Failure(params string[] errors)
        {
            return new ValidationResult 
            { 
                IsValid = false, 
                Errors = new List<string>(errors) 
            };
        }

        public static ValidationResult Failure(List<string> errors)
        {
            return new ValidationResult 
            { 
                IsValid = false, 
                Errors = errors 
            };
        }

        public void AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
        }

        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }

        public void AddAdditionalData(string key, object value)
        {
            AdditionalData[key] = value;
        }
    }
}
