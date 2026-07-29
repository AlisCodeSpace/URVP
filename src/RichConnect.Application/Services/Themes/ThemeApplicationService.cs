using MediatR;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.Interfaces.Themes;
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
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Application.DTOs.Themes;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Services.Themes
{
    /// <summary>
    /// Application service for theme operations - main orchestrator
    /// </summary>
    public class ThemeApplicationService : IThemeApplicationService
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ThemeApplicationService> _logger;
        private readonly IMediator _mediator;
        private readonly IFileReadService _fileReadService;

        public ThemeApplicationService(
            IThemeRepository themeRepository,
            IUserRepository userRepository,
            ILogger<ThemeApplicationService> logger,
            IMediator mediator,
            IFileReadService fileReadService)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _fileReadService = fileReadService ?? throw new ArgumentNullException(nameof(fileReadService));
        }

        #region Commands (Write Operations)

        public async Task<ResearchThemeDto> SubmitThemeAsync(SubmitThemeCommand command)
        {
            try
            {
                _logger.LogInformation("Submitting theme: {Title} by user {UserId}", command.Title, command.SubmittedBy);

                // Validate business rules
                if (command.IsAdminCreated)
                {
                    if (!await CanUserApproveThemesAsync(command.SubmittedBy))
                    {
                        throw new UnauthorizedAccessException($"User {command.SubmittedBy} is not authorized to create themes as admin.");
                    }
                }
                else
                {
                    if (!await CanUserSubmitThemeAsync(command.SubmittedBy))
                    {
                        throw new UnauthorizedAccessException($"User {command.SubmittedBy} is not authorized to submit themes.");
                    }
                }

                // Execute command
                var theme = await _mediator.Send(command);

                // Map to DTO
                var dto = await MapToDtoAsync(theme);
                _logger.LogInformation("Successfully submitted theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting theme: {Title}", command.Title);
                throw;
            }
        }

        public async Task<ResearchThemeDto> ApproveThemeAsync(ApproveThemeCommand command)
        {
            try
            {
                _logger.LogInformation("Approving theme: {ThemeId} by user {UserId}", command.ThemeId, command.ApprovedBy);

                // Validate business rules
                if (!await CanUserApproveThemesAsync(command.ApprovedBy))
                {
                    throw new UnauthorizedAccessException($"User {command.ApprovedBy} is not authorized to approve themes.");
                }

                // Execute command
                var theme = await _mediator.Send(command);

                // Map to DTO
                var dto = await MapToDtoAsync(theme);
                _logger.LogInformation("Successfully approved theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving theme: {ThemeId}", command.ThemeId);
                throw;
            }
        }

        public async Task<ResearchThemeDto> PublishThemeAsync(PublishThemeCommand command)
        {
            try
            {
                _logger.LogInformation("Publishing theme: {ThemeId} by user {UserId}", command.ThemeId, command.PublishedBy);

                // Validate business rules
                if (!await CanUserApproveThemesAsync(command.PublishedBy))
                {
                    throw new UnauthorizedAccessException($"User {command.PublishedBy} is not authorized to publish themes.");
                }

                // Execute command
                var theme = await _mediator.Send(command);

                // Map to DTO
                var dto = await MapToDtoAsync(theme);
                _logger.LogInformation("Successfully published theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing theme: {ThemeId}", command.ThemeId);
                throw;
            }
        }

        public async Task<ResearchThemeDto> UnpublishThemeAsync(UnpublishThemeCommand command)
        {
            try
            {
                _logger.LogInformation("Unpublishing theme: {ThemeId} by user {UserId}", command.ThemeId, command.UnpublishedBy);

                // Validate business rules
                if (!await CanUserApproveThemesAsync(command.UnpublishedBy))
                {
                    throw new UnauthorizedAccessException($"User {command.UnpublishedBy} is not authorized to unpublish themes.");
                }

                // Execute command
                var theme = await _mediator.Send(command);

                // Map to DTO
                var dto = await MapToDtoAsync(theme);
                _logger.LogInformation("Successfully unpublished theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing theme: {ThemeId}", command.ThemeId);
                throw;
            }
        }

        public async Task<ResearchThemeDto> RejectThemeAsync(RejectThemeCommand command)
        {
            try
            {
                _logger.LogInformation("Rejecting theme: {ThemeId} by user {UserId}", command.ThemeId, command.RejectedBy);

                // Validate business rules
                if (!await CanUserApproveThemesAsync(command.RejectedBy))
                {
                    throw new UnauthorizedAccessException($"User {command.RejectedBy} is not authorized to reject themes.");
                }

                // Execute command
                var theme = await _mediator.Send(command);

                // Map to DTO
                var dto = await MapToDtoAsync(theme);
                _logger.LogInformation("Successfully rejected theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting theme: {ThemeId}", command.ThemeId);
                throw;
            }
        }

        public async Task<ResearchThemeDto> UpdateThemeAsync(UpdateThemeCommand command)
        {
            try
            {
                _logger.LogInformation("Updating theme: {ThemeId} by user {UserId}", command.ThemeId, command.UpdatedBy);

                // Validate business rules
                if (!await CanUserUpdateThemeAsync(command.ThemeId, command.UpdatedBy))
                {
                    throw new UnauthorizedAccessException($"User {command.UpdatedBy} is not authorized to update theme {command.ThemeId}.");
                }

                // Execute command
                var theme = await _mediator.Send(command);

                // Map to DTO
                var dto = await MapToDtoAsync(theme);
                _logger.LogInformation("Successfully updated theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating theme: {ThemeId}", command.ThemeId);
                throw;
            }
        }

        public async Task<bool> DeleteThemeAsync(DeleteThemeCommand command)
        {
            try
            {
                _logger.LogInformation("Deleting theme: {ThemeId} by user {UserId}", command.ThemeId, command.DeletedBy);

                // Validate business rules
                if (!await CanUserDeleteThemeAsync(command.ThemeId, command.DeletedBy))
                {
                    throw new UnauthorizedAccessException($"User {command.DeletedBy} is not authorized to delete theme {command.ThemeId}.");
                }

                // Execute command
                var result = await _mediator.Send(command);

                _logger.LogInformation("Successfully deleted theme: {ThemeId}", command.ThemeId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting theme: {ThemeId}", command.ThemeId);
                throw;
            }
        }

        #endregion

        #region Queries (Read Operations)

        public async Task<ResearchThemeDto?> GetThemeByIdAsync(GetThemeByIdQuery query)
        {
            try
            {
                _logger.LogInformation("Getting theme by ID: {ThemeId}", query.ThemeId);

                var theme = await _mediator.Send(query);
                if (theme == null)
                {
                    _logger.LogWarning("Theme not found: {ThemeId}", query.ThemeId);
                    return null;
                }

                var dto = await MapToDtoAsync(theme);
                _logger.LogInformation("Successfully retrieved theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting theme by ID: {ThemeId}", query.ThemeId);
                throw;
            }
        }

        public async Task<ResearchThemeDto?> GetThemeBySlugAsync(GetThemeBySlugQuery query)
        {
            try
            {
                _logger.LogInformation("Getting theme by slug: {Slug}", query.Slug);

                var theme = await _mediator.Send<ResearchTheme?>(query);
                if (theme == null)
                {
                    _logger.LogWarning("Theme not found for slug: {Slug}", query.Slug);
                    return null;
                }

                var dto = await MapToDtoAsync(theme);
                _logger.LogInformation("Successfully retrieved theme: {ThemeId} - {Title} by slug: {Slug}", 
                    theme.Id, theme.Title, query.Slug);
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting theme by slug: {Slug}", query.Slug);
                throw;
            }
        }

        public async Task<List<ResearchThemeDto>> GetThemesByStatusAsync(GetThemesByStatusQuery query)
        {
            try
            {
                _logger.LogInformation("Getting themes by status: {Status}", query.Status);

                var themes = await _mediator.Send<List<ResearchTheme>>(query);
                
                // Map themes to DTOs with error handling for individual themes
                var dtos = new List<ResearchThemeDto>();
                foreach (var theme in themes)
                {
                    try
                    {
                        var dto = await MapToDtoAsync(theme);
                        dtos.Add(dto);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error mapping theme {ThemeId} to DTO, skipping: {Title}", theme.Id, theme.Title);
                        // Continue processing other themes even if one fails
                    }
                }

                _logger.LogInformation("Successfully retrieved {Count} themes with status: {Status}", 
                    dtos.Count, query.Status);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting themes by status: {Status}", query.Status);
                throw;
            }
        }

        public async Task<List<ResearchThemeDto>> GetUserThemesAsync(GetUserThemesQuery query)
        {
            try
            {
                _logger.LogInformation("Getting themes for user: {UserId}", query.UserId);

                var themes = await _mediator.Send<List<ResearchTheme>>(query);
                var dtos = (await Task.WhenAll(themes.Select(MapToDtoAsync))).ToList();

                _logger.LogInformation("Successfully retrieved {Count} themes for user: {UserId}", 
                    dtos.Count, query.UserId);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting themes for user: {UserId}", query.UserId);
                throw;
            }
        }

        public async Task<List<ResearchThemeDto>> GetAllThemesAsync(GetAllThemesQuery query)
        {
            try
            {
                _logger.LogInformation("Getting all themes with filters");

                var themes = await _mediator.Send<List<ResearchTheme>>(query);
                var dtos = (await Task.WhenAll(themes.Select(MapToDtoAsync))).ToList();

                _logger.LogInformation("Successfully retrieved {Count} themes", dtos.Count);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all themes");
                throw;
            }
        }

        #endregion

        #region Business Rules and Validation

        public async Task<bool> CanUserSubmitThemeAsync(Guid userId)
        {
            try
            {
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                if (!userExists)
                    return false;

                // Only Faculty Specialists can submit themes
                var isFacultySpecialist = await _userRepository.HasRoleAsync(userId, UserRole.FacultySpecialist);
                if (!isFacultySpecialist)
                {
                    _logger.LogWarning("User {UserId} attempted to submit theme without faculty specialist role", userId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user submission rights: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> CanUserApproveThemesAsync(Guid userId)
        {
            try
            {
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                if (!userExists)
                    return false;

                // Only Admins can approve themes
                var isAdmin = await _userRepository.HasRoleAsync(userId, UserRole.Admin);
                if (!isAdmin)
                {
                    _logger.LogWarning("User {UserId} attempted to approve themes without admin role", userId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user approval rights: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> CanUserUpdateThemeAsync(Guid themeId, Guid userId)
        {
            try
            {
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                if (!userExists)
                    return false;

                var theme = await _themeRepository.GetByIdAsync(themeId);
                if (theme == null)
                    return false;

                // Admin can update any theme
                var isAdmin = await _userRepository.HasRoleAsync(userId, UserRole.Admin);
                if (isAdmin)
                    return true;

                // Faculty Specialist can update their own themes
                var isFacultySpecialist = await _userRepository.HasRoleAsync(userId, UserRole.FacultySpecialist);
                if (isFacultySpecialist && theme.SubmittedBy == userId)
                    return true;

                _logger.LogWarning("User {UserId} attempted to update theme {ThemeId} without proper permissions", userId, themeId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user update rights: {UserId} for theme {ThemeId}", userId, themeId);
                return false;
            }
        }

        public async Task<bool> CanUserDeleteThemeAsync(Guid themeId, Guid userId)
        {
            try
            {
                var userExists = await _themeRepository.ValidateUserExistsAsync(userId);
                if (!userExists)
                    return false;

                var themeExists = await _themeRepository.ExistsAsync(themeId);
                if (!themeExists)
                    return false;

                // Only Admins can delete themes
                var isAdmin = await _userRepository.HasRoleAsync(userId, UserRole.Admin);
                if (!isAdmin)
                {
                    _logger.LogWarning("User {UserId} attempted to delete theme {ThemeId} without admin role", userId, themeId);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user delete rights: {UserId} for theme {ThemeId}", userId, themeId);
                return false;
            }
        }

        public async Task<ThemeStatisticsDto> GetThemeStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting theme statistics");

                var statusCounts = await _themeRepository.GetStatusCountsAsync();
                var themesByResearchField = await _themeRepository.GetCountsByResearchFieldAsync();
                var themesByUser = await _themeRepository.GetCountsByUserAsync();
                var recentThemes = await _themeRepository.GetRecentlyUpdatedWithIncludesAsync(7);

                var statistics = new ThemeStatisticsDto
                {
                    TotalThemes = statusCounts.Values.Sum(),
                    PendingThemes = statusCounts.GetValueOrDefault(ApprovalStatus.Pending, 0),
                    ApprovedThemes = statusCounts.GetValueOrDefault(ApprovalStatus.Approved, 0),
                    RejectedThemes = statusCounts.GetValueOrDefault(ApprovalStatus.Rejected, 0),
                    ThemesThisMonth = await GetThemesThisMonthAsync(),
                    ThemesThisWeek = await GetThemesThisWeekAsync(),
                    StatusCounts = statusCounts,
                    ThemesByResearchField = themesByResearchField,
                    ThemesByUser = themesByUser,
                    RecentThemes = (await Task.WhenAll(recentThemes.Select(MapToDtoAsync))).ToList()
                };

                _logger.LogInformation("Successfully retrieved theme statistics");
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting theme statistics");
                throw;
            }
        }

        #endregion

        #region Utility Methods

        public async Task<string> GenerateUniqueSlugAsync(string title, Guid? excludeThemeId = null)
        {
            try
            {
                var baseSlug = GenerateSlug(title);
                var slug = baseSlug;
                var counter = 1;

                while (!await IsSlugAvailableAsync(slug, excludeThemeId))
                {
                    slug = $"{baseSlug}-{counter}";
                    counter++;
                }

                return slug;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating unique slug for title: {Title}", title);
                throw;
            }
        }

        public async Task<bool> IsSlugAvailableAsync(string slug, Guid? excludeThemeId = null)
        {
            try
            {
                return await _themeRepository.ValidateSlugIsUniqueAsync(slug, excludeThemeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slug availability: {Slug}", slug);
                return false;
            }
        }

        public async Task<List<ResearchThemeDto>> GetThemesForReviewAsync()
        {
            try
            {
                _logger.LogInformation("Getting themes for review");

                var themes = await _themeRepository.GetForAdminReviewWithIncludesAsync();
                var dtos = (await Task.WhenAll(themes.Select(MapToDtoAsync))).ToList();

                _logger.LogInformation("Successfully retrieved {Count} themes for review", dtos.Count);
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting themes for review");
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<ResearchThemeDto> MapToDtoAsync(ResearchTheme theme)
        {
            // Get file IDs from FileStorage for theme (with error handling)
            Guid? imageFileId = null;
            Guid? documentFileId = null;
            try
            {
                imageFileId = await _fileReadService.GetFileIdByEntityAsync("Theme", theme.Id, "Image");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting image file ID for theme {ThemeId}", theme.Id);
            }
            
            try
            {
                documentFileId = await _fileReadService.GetFileIdByEntityAsync("Theme", theme.Id, "Document");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting document file ID for theme {ThemeId}", theme.Id);
            }
            
            return new ResearchThemeDto
            {
                Id = theme.Id,
                Title = theme.Title,
                Slug = theme.Slug,
                Description = theme.Description,
                ExpectedOutcomes = theme.ExpectedOutcomes,
                EstimatedFunding = theme.EstimatedFunding,
                Status = theme.Status,
                IsPublished = theme.IsPublished,
                SubmittedBy = theme.SubmittedBy,
                ApprovedBy = theme.ApprovedBy,
                ResearchFieldId = theme.ResearchFieldId,
                ResearchField = theme.ResearchField != null ? new ResearchFieldDto
                {
                    Id = theme.ResearchField.Id,
                    Name = theme.ResearchField.Name,
                    Slug = theme.ResearchField.Slug,
                    Category = theme.ResearchField.Category,
                    DisplayOrder = theme.ResearchField.DisplayOrder,
                    IsActive = theme.ResearchField.IsActive,
                    CreatedAt = theme.ResearchField.CreatedAt,
                    UpdatedAt = theme.ResearchField.UpdatedAt
                } : null,
                ImageUrl = imageFileId?.ToString(),
                DocumentUrl = documentFileId?.ToString(),
                CreatedAt = theme.CreatedAt,
                UpdatedAt = theme.UpdatedAt
            };
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            var slug = title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(",", "")
                .Replace(".", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("/", "-")
                .Replace("\\", "-")
                .Replace("+", "plus")
                .Replace("=", "equals")
                .Replace("@", "at")
                .Replace("#", "hash")
                .Replace("$", "dollar")
                .Replace("%", "percent")
                .Replace("^", "")
                .Replace("*", "")
                .Replace("_", "-");

            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }

            return slug.Trim('-');
        }

        private async Task<int> GetThemesThisMonthAsync()
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            var themes = await _themeRepository.GetByDateRangeAsync(startOfMonth, endOfMonth);
            return themes.Count;
        }

        private async Task<int> GetThemesThisWeekAsync()
        {
            var startOfWeek = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);
            var themes = await _themeRepository.GetByDateRangeAsync(startOfWeek, endOfWeek);
            return themes.Count;
        }

        #endregion
    }
}
