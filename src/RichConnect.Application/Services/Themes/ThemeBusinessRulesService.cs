using RICHConnect.Backend.Application.Interfaces.Themes;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Services.Themes
{
    /// <summary>
    /// Business rules service for theme operations
    /// </summary>
    public class ThemeBusinessRulesService : IThemeBusinessRulesService
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ThemeBusinessRulesService> _logger;

        // Business rule constants
        private const int MaxThemesPerUser = 10;
        private const int MaxTitleLength = 128;
        private const int MaxDescriptionLength = 5000;
        private const int MaxExpectedOutcomesLength = 2000;
        private const int MaxRejectionReasonLength = 1000;
        private const int MinRejectionReasonLength = 10;
        private const int MaxImageSizeInMb = 5;
        private const int MaxDocumentSizeInMb = 10;
        private readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly string[] AllowedDocumentExtensions = { ".pdf", ".doc", ".docx", ".txt" };

        public ThemeBusinessRulesService(
            IThemeRepository themeRepository,
            IUserRepository userRepository,
            ILogger<ThemeBusinessRulesService> logger)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Theme Submission Rules

        public async Task<ValidationResult> CanUserSubmitThemeAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Validating user theme submission rights: {UserId}", userId);

                var result = new ValidationResult { IsValid = true };

                // Check if user exists
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                if (!userExists)
                {
                    result.AddError("User does not exist or is not authorized to submit themes.");
                    return result;
                }

                // Check submission limit
                var submissionLimitResult = await CheckThemeSubmissionLimitAsync(userId);
                if (!submissionLimitResult.IsValid)
                {
                    result.Errors.AddRange(submissionLimitResult.Errors);
                    result.IsValid = false;
                }

                _logger.LogInformation("User theme submission validation completed: {UserId}, Valid: {IsValid}", 
                    userId, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user theme submission rights: {UserId}", userId);
                return ValidationResult.Failure($"Error validating submission rights: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateThemeSubmissionAsync(FacultySpecialistThemeSubmissionDto dto, Guid userId)
        {
            try
            {
                _logger.LogInformation("Validating theme submission: {Title} by user {UserId}", dto.Title, userId);

                var result = new ValidationResult { IsValid = true };

                // Validate title
                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    result.AddError("Theme title is required.");
                }
                else if (dto.Title.Length > MaxTitleLength)
                {
                    result.AddError($"Theme title cannot exceed {MaxTitleLength} characters.");
                }

                // Validate description
                if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > MaxDescriptionLength)
                {
                    result.AddError($"Description cannot exceed {MaxDescriptionLength} characters.");
                }

                // Validate expected outcomes
                if (!string.IsNullOrEmpty(dto.ExpectedOutcomes) && dto.ExpectedOutcomes.Length > MaxExpectedOutcomesLength)
                {
                    result.AddError($"Expected outcomes cannot exceed {MaxExpectedOutcomesLength} characters.");
                }

                // Validate estimated funding
                var fundingResult = await ValidateEstimatedFundingAsync(dto.EstimatedFunding);
                if (!fundingResult.IsValid)
                {
                    result.Errors.AddRange(fundingResult.Errors);
                }

                // Validate research field
                if (dto.ResearchFieldId.HasValue)
                {
                    var researchFieldExists = await _themeRepository.ValidateResearchFieldExistsAsync(dto.ResearchFieldId.Value);
                    if (!researchFieldExists)
                    {
                        result.AddError("Selected research field does not exist.");
                    }
                }

                // Validate file uploads
                var fileResult = await ValidateFileUploadsAsync(null, dto.Document);
                if (!fileResult.IsValid)
                {
                    result.Errors.AddRange(fileResult.Errors);
                }

                // Validate content quality
                var contentResult = await ValidateThemeContentQualityAsync(dto.Title, dto.Description, dto.ExpectedOutcomes);
                if (!contentResult.IsValid)
                {
                    result.Errors.AddRange(contentResult.Errors);
                }

                // Check title uniqueness
                var titleResult = await ValidateTitleUniquenessAsync(dto.Title);
                if (!titleResult.IsValid)
                {
                    result.Errors.AddRange(titleResult.Errors);
                }

                result.IsValid = result.Errors.Count == 0;

                _logger.LogInformation("Theme submission validation completed: {Title}, Valid: {IsValid}", 
                    dto.Title, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme submission: {Title}", dto.Title);
                return ValidationResult.Failure($"Error validating theme submission: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateAdminThemeCreationAsync(AdminThemeCreationDto dto, Guid adminUserId)
        {
            try
            {
                _logger.LogInformation("Validating admin theme creation: {Title} by admin {AdminId}", dto.Title, adminUserId);

                var result = new ValidationResult { IsValid = true };

                // Validate admin privileges
                var adminResult = await ValidateAdminPrivilegesAsync(adminUserId);
                if (!adminResult.IsValid)
                {
                    result.Errors.AddRange(adminResult.Errors);
                    result.IsValid = false;
                    return result;
                }

                // Validate basic theme data (similar to facultySpecialist submission)
                var basicValidation = await ValidateThemeSubmissionAsync(new FacultySpecialistThemeSubmissionDto
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    ExpectedOutcomes = dto.ExpectedOutcomes,
                    EstimatedFunding = dto.EstimatedFunding,
                    ResearchFieldId = dto.ResearchFieldId
                }, adminUserId);

                if (!basicValidation.IsValid)
                {
                    result.Errors.AddRange(basicValidation.Errors);
                }

                // Validate image upload
                var imageResult = await ValidateFileUploadsAsync(dto.Image, null);
                if (!imageResult.IsValid)
                {
                    result.Errors.AddRange(imageResult.Errors);
                }

                result.IsValid = result.Errors.Count == 0;

                _logger.LogInformation("Admin theme creation validation completed: {Title}, Valid: {IsValid}", 
                    dto.Title, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating admin theme creation: {Title}", dto.Title);
                return ValidationResult.Failure($"Error validating admin theme creation: {ex.Message}");
            }
        }

        public async Task<ValidationResult> CheckThemeSubmissionLimitAsync(Guid userId)
        {
            try
            {
                var userThemeCount = await _themeRepository.GetCountByUserAsync(userId);
                
                if (userThemeCount >= MaxThemesPerUser)
                {
                    return ValidationResult.Failure($"User has reached the maximum limit of {MaxThemesPerUser} themes.");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking theme submission limit: {UserId}", userId);
                return ValidationResult.Failure($"Error checking submission limit: {ex.Message}");
            }
        }

        #endregion

        #region Theme Approval/Rejection Rules

        public async Task<ValidationResult> CanUserApproveThemesAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Validating user theme approval rights: {UserId}", userId);

                var result = new ValidationResult { IsValid = true };

                // Check if user exists
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                if (!userExists)
                {
                    result.AddError("User does not exist or is not authorized to approve themes.");
                    return result;
                }

                // Role-based validation: Only Admin can approve themes
                var isAdmin = await _userRepository.HasRoleAsync(userId, UserRole.Admin);
                if (!isAdmin)
                {
                    result.AddError("Only administrators can approve themes.");
                    _logger.LogWarning("User {UserId} attempted to approve themes without admin privileges", userId);
                    return result;
                }

                _logger.LogInformation("User theme approval validation completed: {UserId}, Valid: {IsValid}", 
                    userId, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user theme approval rights: {UserId}", userId);
                return ValidationResult.Failure($"Error validating approval rights: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateThemeApprovalAsync(Guid themeId, Guid approverId)
        {
            try
            {
                _logger.LogInformation("Validating theme approval: {ThemeId} by {ApproverId}", themeId, approverId);

                var result = new ValidationResult { IsValid = true };

                // Check if theme exists
                var theme = await _themeRepository.GetByIdAsync(themeId);
                if (theme == null)
                {
                    result.AddError("Theme does not exist.");
                    return result;
                }

                // Check if theme is in pending status
                if (theme.Status != ApprovalStatus.Pending)
                {
                    result.AddError($"Theme is not in pending status. Current status: {theme.Status}");
                    return result;
                }

                // Validate approver rights
                var approverResult = await CanUserApproveThemesAsync(approverId);
                if (!approverResult.IsValid)
                {
                    result.Errors.AddRange(approverResult.Errors);
                    result.IsValid = false;
                }

                _logger.LogInformation("Theme approval validation completed: {ThemeId}, Valid: {IsValid}", 
                    themeId, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme approval: {ThemeId}", themeId);
                return ValidationResult.Failure($"Error validating theme approval: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateThemeRejectionAsync(Guid themeId, Guid rejectorId, string rejectionReason)
        {
            try
            {
                _logger.LogInformation("Validating theme rejection: {ThemeId} by {RejectorId}", themeId, rejectorId);

                var result = new ValidationResult { IsValid = true };

                // Validate rejection reason
                if (string.IsNullOrWhiteSpace(rejectionReason))
                {
                    result.AddError("Rejection reason is required.");
                }
                else if (rejectionReason.Length < MinRejectionReasonLength)
                {
                    result.AddError($"Rejection reason must be at least {MinRejectionReasonLength} characters long.");
                }
                else if (rejectionReason.Length > MaxRejectionReasonLength)
                {
                    result.AddError($"Rejection reason cannot exceed {MaxRejectionReasonLength} characters.");
                }

                // Check if theme exists and is in pending status
                var themeResult = await ValidateThemeStateForActionAsync(themeId, "rejection");
                if (!themeResult.IsValid)
                {
                    result.Errors.AddRange(themeResult.Errors);
                }

                // Validate rejector rights
                var rejectorResult = await CanUserApproveThemesAsync(rejectorId);
                if (!rejectorResult.IsValid)
                {
                    result.Errors.AddRange(rejectorResult.Errors);
                }

                result.IsValid = result.Errors.Count == 0;

                _logger.LogInformation("Theme rejection validation completed: {ThemeId}, Valid: {IsValid}", 
                    themeId, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme rejection: {ThemeId}", themeId);
                return ValidationResult.Failure($"Error validating theme rejection: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateThemeStateForActionAsync(Guid themeId, string action)
        {
            try
            {
                var theme = await _themeRepository.GetByIdAsync(themeId);
                if (theme == null)
                {
                    return ValidationResult.Failure("Theme does not exist.");
                }

                if (theme.Status != ApprovalStatus.Pending)
                {
                    return ValidationResult.Failure($"Theme is not in pending status and cannot be {action}. Current status: {theme.Status}");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme state: {ThemeId}", themeId);
                return ValidationResult.Failure($"Error validating theme state: {ex.Message}");
            }
        }

        #endregion

        #region Theme Update Rules

        public async Task<ValidationResult> CanUserUpdateThemeAsync(Guid themeId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Validating user theme update rights: {UserId} for theme {ThemeId}", userId, themeId);

                var result = new ValidationResult { IsValid = true };

                // Check if user exists
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                if (!userExists)
                {
                    result.AddError("User does not exist or is not authorized to update themes.");
                    return result;
                }

                // Check if theme exists
                var theme = await _themeRepository.GetByIdAsync(themeId);
                if (theme == null)
                {
                    result.AddError("Theme does not exist.");
                    return result;
                }

                // Role-based validation: Admin can update any theme, Faculty Specialist can only update their own
                var isAdmin = await _userRepository.HasRoleAsync(userId, UserRole.Admin);
                var isFacultySpecialist = await _userRepository.HasRoleAsync(userId, UserRole.FacultySpecialist);
                
                if (!isAdmin && !isFacultySpecialist)
                {
                    result.AddError("Only administrators and faculty specialists can update themes.");
                    return result;
                }

                // If not admin, check if user owns the theme
                if (!isAdmin && theme.SubmittedBy != userId)
                {
                    result.AddError("You can only update your own themes.");
                    _logger.LogWarning("User {UserId} attempted to update theme {ThemeId} without ownership", userId, themeId);
                    return result;
                }

                _logger.LogInformation("User theme update validation completed: {UserId} for {ThemeId}, Valid: {IsValid}", 
                    userId, themeId, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user theme update rights: {UserId} for {ThemeId}", userId, themeId);
                return ValidationResult.Failure($"Error validating update rights: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateThemeUpdateAsync(AdminThemeUpdateDto dto, Guid themeId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Validating theme update: {ThemeId} by {UserId}", themeId, userId);

                var result = new ValidationResult { IsValid = true };

                // Validate basic data
                if (string.IsNullOrWhiteSpace(dto.Title))
                {
                    result.AddError("Theme title is required.");
                }
                else if (dto.Title.Length > MaxTitleLength)
                {
                    result.AddError($"Theme title cannot exceed {MaxTitleLength} characters.");
                }

                if (!string.IsNullOrEmpty(dto.Description) && dto.Description.Length > MaxDescriptionLength)
                {
                    result.AddError($"Description cannot exceed {MaxDescriptionLength} characters.");
                }

                if (!string.IsNullOrEmpty(dto.ExpectedOutcomes) && dto.ExpectedOutcomes.Length > MaxExpectedOutcomesLength)
                {
                    result.AddError($"Expected outcomes cannot exceed {MaxExpectedOutcomesLength} characters.");
                }

                // Validate estimated funding
                var fundingResult = await ValidateEstimatedFundingAsync(dto.EstimatedFunding);
                if (!fundingResult.IsValid)
                {
                    result.Errors.AddRange(fundingResult.Errors);
                }

                // Validate research field
                if (dto.ResearchFieldId.HasValue)
                {
                    var researchFieldExists = await _themeRepository.ValidateResearchFieldExistsAsync(dto.ResearchFieldId.Value);
                    if (!researchFieldExists)
                    {
                        result.AddError("Selected research field does not exist.");
                    }
                }

                // Validate file uploads
                var fileResult = await ValidateFileUploadsAsync(dto.Image, null);
                if (!fileResult.IsValid)
                {
                    result.Errors.AddRange(fileResult.Errors);
                }

                // Check if theme can be updated
                var updateabilityResult = await ValidateThemeUpdateabilityAsync(themeId);
                if (!updateabilityResult.IsValid)
                {
                    result.Errors.AddRange(updateabilityResult.Errors);
                }

                result.IsValid = result.Errors.Count == 0;

                _logger.LogInformation("Theme update validation completed: {ThemeId}, Valid: {IsValid}", 
                    themeId, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme update: {ThemeId}", themeId);
                return ValidationResult.Failure($"Error validating theme update: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateThemeUpdateabilityAsync(Guid themeId)
        {
            try
            {
                var theme = await _themeRepository.GetByIdAsync(themeId);
                if (theme == null)
                {
                    return ValidationResult.Failure("Theme does not exist.");
                }

                // Check if theme has dependencies that would be affected by updates
                var hasDependencies = await _themeRepository.HasDependenciesAsync(themeId);
                
                if (hasDependencies && theme.Status == ApprovalStatus.Approved)
                {
                    // Approved themes with active dependencies require extra caution
                    // Allow updates but log a warning
                    _logger.LogWarning("Theme {ThemeId} is being updated while having active dependencies (challenges/projects)", themeId);
                }

                // Rejected themes cannot be updated directly - they need to be resubmitted
                if (theme.Status == ApprovalStatus.Rejected)
                {
                    return ValidationResult.Failure("Rejected themes cannot be updated. Please submit a new theme instead.");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme updateability: {ThemeId}", themeId);
                return ValidationResult.Failure($"Error validating theme updateability: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateSlugUniquenessAsync(string slug, Guid? excludeThemeId = null)
        {
            try
            {
                var isUnique = await _themeRepository.ValidateSlugIsUniqueAsync(slug, excludeThemeId);
                
                if (!isUnique)
                {
                    return ValidationResult.Failure("Theme slug must be unique.");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating slug uniqueness: {Slug}", slug);
                return ValidationResult.Failure($"Error validating slug uniqueness: {ex.Message}");
            }
        }

        #endregion

        #region Theme Deletion Rules

        public async Task<ValidationResult> CanUserDeleteThemeAsync(Guid themeId, Guid userId)
        {
            try
            {
                _logger.LogInformation("Validating user theme deletion rights: {UserId} for theme {ThemeId}", userId, themeId);

                var result = new ValidationResult { IsValid = true };

                // Check if user exists
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                if (!userExists)
                {
                    result.AddError("User does not exist or is not authorized to delete themes.");
                    return result;
                }

                // Check if theme exists
                var theme = await _themeRepository.GetByIdAsync(themeId);
                if (theme == null)
                {
                    result.AddError("Theme does not exist.");
                    return result;
                }

                // Role-based validation: Only Admin can delete themes
                var isAdmin = await _userRepository.HasRoleAsync(userId, UserRole.Admin);
                if (!isAdmin)
                {
                    result.AddError("Only administrators can delete themes.");
                    _logger.LogWarning("User {UserId} attempted to delete theme {ThemeId} without admin privileges", userId, themeId);
                    return result;
                }

                _logger.LogInformation("User theme deletion validation completed: {UserId} for {ThemeId}, Valid: {IsValid}", 
                    userId, themeId, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user theme deletion rights: {UserId} for {ThemeId}", userId, themeId);
                return ValidationResult.Failure($"Error validating deletion rights: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateThemeDeletionAsync(Guid themeId, Guid deleterId)
        {
            try
            {
                _logger.LogInformation("Validating theme deletion: {ThemeId} by {DeleterId}", themeId, deleterId);

                var result = new ValidationResult { IsValid = true };

                // Check if theme exists
                var theme = await _themeRepository.GetByIdAsync(themeId);
                if (theme == null)
                {
                    result.AddError("Theme does not exist.");
                    return result;
                }

                // Validate deleter rights
                var deleterResult = await CanUserDeleteThemeAsync(themeId, deleterId);
                if (!deleterResult.IsValid)
                {
                    result.Errors.AddRange(deleterResult.Errors);
                }

                // Check dependencies
                var dependencyResult = await ValidateThemeDeletionDependenciesAsync(themeId);
                if (!dependencyResult.IsValid)
                {
                    result.Errors.AddRange(dependencyResult.Errors);
                }

                result.IsValid = result.Errors.Count == 0;

                _logger.LogInformation("Theme deletion validation completed: {ThemeId}, Valid: {IsValid}", 
                    themeId, result.IsValid);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme deletion: {ThemeId}", themeId);
                return ValidationResult.Failure($"Error validating theme deletion: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateThemeDeletionDependenciesAsync(Guid themeId)
        {
            try
            {
                var hasDependencies = await _themeRepository.HasDependenciesAsync(themeId);
                
                if (hasDependencies)
                {
                    return ValidationResult.Failure("This theme cannot be deleted because it has associated challenges or R&D projects. Please remove or reassign these dependencies first.");
                }
                
                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme deletion dependencies: {ThemeId}", themeId);
                return ValidationResult.Failure($"Error validating theme deletion dependencies: {ex.Message}");
            }
        }

        #endregion

        #region General Business Rules

        public async Task<ValidationResult> ValidateResearchFieldAssignmentAsync(Guid? researchFieldId)
        {
            try
            {
                if (!researchFieldId.HasValue)
                {
                    return ValidationResult.Success();
                }

                var researchFieldExists = await _themeRepository.ValidateResearchFieldExistsAsync(researchFieldId.Value);
                if (!researchFieldExists)
                {
                    return ValidationResult.Failure("Selected research field does not exist.");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating research field assignment: {ResearchFieldId}", researchFieldId);
                return ValidationResult.Failure($"Error validating research field assignment: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateEstimatedFundingAsync(double estimatedFunding)
        {
            try
            {
                await Task.CompletedTask;
                var result = new ValidationResult { IsValid = true };

                if (estimatedFunding < 0)
                {
                    result.AddError("Estimated funding cannot be negative.");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating estimated funding: {EstimatedFunding}", estimatedFunding);
                return ValidationResult.Failure($"Error validating estimated funding: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateFileUploadsAsync(IFormFile? image, IFormFile? document)
        {
            try
            {
                await Task.CompletedTask;
                var result = new ValidationResult { IsValid = true };

                // Validate image
                if (image != null && image.Length > 0)
                {
                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                    if (!AllowedImageExtensions.Contains(extension))
                    {
                        result.AddError($"Image file must be one of the following types: {string.Join(", ", AllowedImageExtensions)}");
                    }

                    if (image.Length > MaxImageSizeInMb * 1024 * 1024)
                    {
                        result.AddError($"Image file size must not exceed {MaxImageSizeInMb}MB.");
                    }
                }

                // Validate document
                if (document != null && document.Length > 0)
                {
                    var extension = Path.GetExtension(document.FileName).ToLowerInvariant();
                    if (!AllowedDocumentExtensions.Contains(extension))
                    {
                        result.AddError($"Document file must be one of the following types: {string.Join(", ", AllowedDocumentExtensions)}");
                    }

                    if (document.Length > MaxDocumentSizeInMb * 1024 * 1024)
                    {
                        result.AddError($"Document file size must not exceed {MaxDocumentSizeInMb}MB.");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file uploads");
                return ValidationResult.Failure($"Error validating file uploads: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateTitleUniquenessAsync(string title, Guid? excludeThemeId = null)
        {
            try
            {
                var isUnique = await _themeRepository.ValidateTitleIsUniqueAsync(title, excludeThemeId);
                
                if (!isUnique)
                {
                    return ValidationResult.Failure("A theme with this title already exists. Please choose a different title.");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating title uniqueness: {Title}", title);
                return ValidationResult.Failure($"Error validating title uniqueness: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateThemeContentQualityAsync(string title, string? description, string? expectedOutcomes)
        {
            try
            {
                await Task.CompletedTask;
                var result = new ValidationResult { IsValid = true };

                // Basic content quality checks
                if (string.IsNullOrWhiteSpace(title))
                {
                    result.AddError("Theme title is required.");
                }
                else if (title.Length < 5)
                {
                    result.AddError("Theme title must be at least 5 characters long.");
                }
                else if (title.Length > MaxTitleLength)
                {
                    result.AddError($"Theme title cannot exceed {MaxTitleLength} characters.");
                }

                // Check for repetitive characters (spam detection)
                if (!string.IsNullOrWhiteSpace(title) && HasExcessiveRepetition(title))
                {
                    result.AddError("Theme title contains excessive repetition. Please provide a meaningful title.");
                }

                // Check minimum description length for better quality
                if (!string.IsNullOrWhiteSpace(description) && description.Length < 20)
                {
                    result.AddError("Description should be at least 20 characters to provide meaningful information.");
                }

                // Check for suspicious patterns (all caps, excessive punctuation)
                if (!string.IsNullOrWhiteSpace(title) && title == title.ToUpper() && title.Length > 10)
                {
                    result.AddError("Please avoid using all uppercase letters in the title.");
                }

                // Check for excessive special characters
                if (!string.IsNullOrWhiteSpace(title))
                {
                    var specialCharCount = title.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
                    var ratio = (double)specialCharCount / title.Length;
                    if (ratio > 0.3)
                    {
                        result.AddError("Title contains too many special characters. Please use a more descriptive title.");
                    }
                }

                // Ensure description and expected outcomes are meaningful if provided
                if (!string.IsNullOrWhiteSpace(description) && description.Trim().Split(' ').Length < 5)
                {
                    result.AddError("Description should contain at least 5 words to be meaningful.");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme content quality");
                return ValidationResult.Failure($"Error validating theme content quality: {ex.Message}");
            }
        }

        private bool HasExcessiveRepetition(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 4)
                return false;

            // Check for character repetition (e.g., "aaaa", "!!!!")
            for (int i = 0; i < text.Length - 3; i++)
            {
                if (text[i] == text[i + 1] && text[i] == text[i + 2] && text[i] == text[i + 3])
                {
                    return true;
                }
            }

            // Check for word repetition
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 3)
            {
                for (int i = 0; i < words.Length - 2; i++)
                {
                    if (words[i].Equals(words[i + 1], StringComparison.OrdinalIgnoreCase) &&
                        words[i].Equals(words[i + 2], StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Role-Based Validation

        public async Task<ValidationResult> ValidateAdminPrivilegesAsync(Guid userId)
        {
            try
            {
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                if (!userExists)
                {
                    return ValidationResult.Failure("User does not exist or is not authorized for admin operations.");
                }

                var isAdmin = await _userRepository.HasRoleAsync(userId, UserRole.Admin);
                if (!isAdmin)
                {
                    _logger.LogWarning("User {UserId} attempted admin operation without admin privileges", userId);
                    return ValidationResult.Failure("Only administrators can perform this operation.");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating admin privileges: {UserId}", userId);
                return ValidationResult.Failure($"Error validating admin privileges: {ex.Message}");
            }
        }

        public async Task<ValidationResult> ValidateFacultySpecialistPrivilegesAsync(Guid userId)
        {
            try
            {
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                if (!userExists)
                {
                    return ValidationResult.Failure("User does not exist or is not authorized for faculty specialist operations.");
                }

                var isFacultySpecialist = await _userRepository.HasRoleAsync(userId, UserRole.FacultySpecialist);
                if (!isFacultySpecialist)
                {
                    _logger.LogWarning("User {UserId} attempted faculty specialist operation without proper role", userId);
                    return ValidationResult.Failure("Only faculty specialists can perform this operation.");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating faculty specialist privileges: {UserId}", userId);
                return ValidationResult.Failure($"Error validating faculty specialist privileges: {ex.Message}");
            }
        }

        public async Task<ValidationResult> CanUserAccessThemeAsync(Guid themeId, Guid userId)
        {
            try
            {
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                var theme = await _themeRepository.GetByIdAsync(themeId);

                if (!userExists)
                {
                    return ValidationResult.Failure("User does not exist or is not authorized to access themes.");
                }

                if (theme == null)
                {
                    return ValidationResult.Failure("Theme does not exist.");
                }

                // Access control: Admin can access all, Faculty Specialist can access approved themes + their own
                var userRole = await _userRepository.GetUserRoleAsync(userId);
                
                if (userRole == UserRole.Admin)
                {
                    // Admin can access all themes
                    return ValidationResult.Success();
                }
                else if (userRole == UserRole.FacultySpecialist)
                {
                    // Faculty specialist can access approved themes or their own themes
                    if (theme.Status == ApprovalStatus.Approved || theme.SubmittedBy == userId)
                    {
                        return ValidationResult.Success();
                    }
                    else
                    {
                        return ValidationResult.Failure("You can only access approved themes or your own themes.");
                    }
                }
                else
                {
                    // Other roles can only access approved themes
                    if (theme.Status == ApprovalStatus.Approved)
                    {
                        return ValidationResult.Success();
                    }
                    else
                    {
                        return ValidationResult.Failure("Only approved themes are accessible.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme access: {UserId} for {ThemeId}", userId, themeId);
                return ValidationResult.Failure($"Error validating theme access: {ex.Message}");
            }
        }

        #endregion

        #region Workflow Rules

        public async Task<ValidationResult> ValidateThemeWorkflowTransitionAsync(Guid themeId, ApprovalStatus fromStatus, ApprovalStatus toStatus, Guid userId)
        {
            try
            {
                await Task.CompletedTask;
                var result = new ValidationResult { IsValid = true };

                // Validate valid transitions
                switch (fromStatus)
                {
                    case ApprovalStatus.Pending:
                        if (toStatus != ApprovalStatus.Approved && toStatus != ApprovalStatus.Rejected)
                        {
                            result.AddError($"Invalid transition from {fromStatus} to {toStatus}.");
                        }
                        break;
                    case ApprovalStatus.Approved:
                        if (toStatus != ApprovalStatus.Pending)
                        {
                            result.AddError($"Invalid transition from {fromStatus} to {toStatus}.");
                        }
                        break;
                    case ApprovalStatus.Rejected:
                        if (toStatus != ApprovalStatus.Pending)
                        {
                            result.AddError($"Invalid transition from {fromStatus} to {toStatus}.");
                        }
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme workflow transition: {ThemeId}", themeId);
                return ValidationResult.Failure($"Error validating theme workflow transition: {ex.Message}");
            }
        }

        public async Task<ValidationResult> CanThemeBePublishedAsync(Guid themeId)
        {
            try
            {
                var theme = await _themeRepository.GetByIdAsync(themeId);
                if (theme == null)
                {
                    return ValidationResult.Failure("Theme does not exist.");
                }

                if (theme.Status != ApprovalStatus.Approved)
                {
                    return ValidationResult.Failure("Only approved themes can be published.");
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme publishability: {ThemeId}", themeId);
                return ValidationResult.Failure($"Error validating theme publishability: {ex.Message}");
            }
        }

        public async Task<ValidationResult> CanThemeBeArchivedAsync(Guid themeId)
        {
            try
            {
                var theme = await _themeRepository.GetByIdAsync(themeId);
                if (theme == null)
                {
                    return ValidationResult.Failure("Theme does not exist.");
                }

                // Only rejected themes or very old pending themes can be archived
                if (theme.Status == ApprovalStatus.Approved)
                {
                    // Check if approved theme has active dependencies
                    var hasDependencies = await _themeRepository.HasDependenciesAsync(themeId);
                    if (hasDependencies)
                    {
                        return ValidationResult.Failure("Approved themes with active challenges or projects cannot be archived.");
                    }
                }

                // Pending themes can only be archived if they're older than 90 days
                if (theme.Status == ApprovalStatus.Pending)
                {
                    var daysSinceCreation = (DateTime.UtcNow - theme.CreatedAt).TotalDays;
                    if (daysSinceCreation < 90)
                    {
                        return ValidationResult.Failure("Pending themes can only be archived after 90 days of inactivity.");
                    }
                }

                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating theme archivability: {ThemeId}", themeId);
                return ValidationResult.Failure($"Error validating theme archivability: {ex.Message}");
            }
        }

        #endregion
    }
}

