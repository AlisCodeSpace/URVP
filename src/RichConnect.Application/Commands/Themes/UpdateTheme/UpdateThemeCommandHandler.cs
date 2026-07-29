using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.Interfaces.Themes;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Commands.Themes.UpdateTheme
{
    public class UpdateThemeCommandHandler : BaseCommandHandler<UpdateThemeCommand, ResearchTheme>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IResearchFieldRepository _researchFieldRepository;
        private readonly IEventBus _eventBus;
        private readonly IFileUploadService _fileUploadService;
        private readonly IFileReadService _fileReadService;
        private readonly IThemeBusinessRulesService _businessRulesService;

        public UpdateThemeCommandHandler(
            ILogger<UpdateThemeCommandHandler> logger,
            AppDbContext context,
            IThemeRepository themeRepository,
            IUserRepository userRepository,
            IResearchFieldRepository researchFieldRepository,
            IEventBus eventBus,
            IFileUploadService fileUploadService,
            IFileReadService fileReadService,
            IThemeBusinessRulesService businessRulesService)
            : base(logger, context)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _researchFieldRepository = researchFieldRepository ?? throw new ArgumentNullException(nameof(researchFieldRepository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _fileReadService = fileReadService ?? throw new ArgumentNullException(nameof(fileReadService));
            _businessRulesService = businessRulesService ?? throw new ArgumentNullException(nameof(businessRulesService));
        }

        protected override async Task<ResearchTheme> HandleInternal(UpdateThemeCommand request, CancellationToken cancellationToken)
        {
            // Validation is handled by ValidationBehavior and UpdateThemeCommandValidator
            var theme = await _themeRepository.GetByIdAsync(request.ThemeId);
            if (theme == null)
            {
                throw new InvalidOperationException($"Theme with ID {request.ThemeId} not found.");
            }

            // Validate user can update this theme
            var canUpdate = await _businessRulesService.CanUserUpdateThemeAsync(request.ThemeId, request.UpdatedBy);
            if (!canUpdate.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", canUpdate.Errors));
            }

            // Validate theme updateability (e.g., rejected themes cannot be updated)
            var updateability = await _businessRulesService.ValidateThemeUpdateabilityAsync(request.ThemeId);
            if (!updateability.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", updateability.Errors));
            }

            // If title is being changed, validate title uniqueness
            if (!string.IsNullOrEmpty(request.Title) && request.Title != theme.Title)
            {
                var titleValidation = await _businessRulesService.ValidateTitleUniquenessAsync(request.Title, theme.Id);
                if (!titleValidation.IsValid)
                {
                    throw new InvalidOperationException(string.Join("; ", titleValidation.Errors));
                }
            }

            // Validate content quality if title, description, or outcomes are being updated
            if (!string.IsNullOrEmpty(request.Title) || request.Description != null || request.ExpectedOutcomes != null)
            {
                var contentValidation = await _businessRulesService.ValidateThemeContentQualityAsync(
                    request.Title ?? theme.Title, 
                    request.Description ?? theme.Description, 
                    request.ExpectedOutcomes ?? theme.ExpectedOutcomes);
                if (!contentValidation.IsValid)
                {
                    throw new InvalidOperationException(string.Join("; ", contentValidation.Errors));
                }
            }

            // Validate estimated funding if being updated
            if (request.EstimatedFunding.HasValue)
            {
                var fundingValidation = await _businessRulesService.ValidateEstimatedFundingAsync(request.EstimatedFunding.Value);
                if (!fundingValidation.IsValid)
                {
                    throw new InvalidOperationException(string.Join("; ", fundingValidation.Errors));
                }
            }

            // Validate research field if being updated
            if (request.ResearchFieldId.HasValue && request.ResearchFieldId.Value != theme.ResearchFieldId)
            {
                var fieldValidation = await _businessRulesService.ValidateResearchFieldAssignmentAsync(request.ResearchFieldId);
                if (!fieldValidation.IsValid)
                {
                    throw new InvalidOperationException(string.Join("; ", fieldValidation.Errors));
                }
            }

                // Store old values for audit
                var oldImageFileId = await _fileReadService.GetFileIdByEntityAsync("Theme", theme.Id, "Image");
                var oldDocumentFileId = await _fileReadService.GetFileIdByEntityAsync("Theme", theme.Id, "Document");
                
                var oldValues = new Dictionary<string, object>
                {
                    ["Title"] = theme.Title,
                    ["Description"] = theme.Description ?? "",
                    ["ExpectedOutcomes"] = theme.ExpectedOutcomes ?? "",
                    ["EstimatedFunding"] = theme.EstimatedFunding,
                    ["ResearchFieldId"] = theme.ResearchFieldId?.ToString() ?? "",
                    ["ImageUrl"] = oldImageFileId?.ToString() ?? "",
                    ["DocumentUrl"] = oldDocumentFileId?.ToString() ?? ""
                };

                var changedFields = new List<string>();
                var newValues = new Dictionary<string, object>();

            // Update properties if provided
            if (!string.IsNullOrEmpty(request.Title) && request.Title != theme.Title)
            {
                theme.Title = request.Title.Trim();
                // Generate unique slug when title changes
                var baseSlug = GenerateSlug(request.Title.Trim());
                var slug = baseSlug;
                var counter = 1;
                
                // Ensure slug uniqueness (excluding current theme)
                while (!await _themeRepository.ValidateSlugIsUniqueAsync(slug, theme.Id))
                {
                    slug = $"{baseSlug}-{counter}";
                    counter++;
                }
                
                theme.Slug = slug;
                changedFields.Add("Title");
                newValues["Title"] = theme.Title;
            }

            if (request.Description != null && request.Description != theme.Description)
            {
                theme.Description = request.Description.Trim();
                changedFields.Add("Description");
                newValues["Description"] = theme.Description;
            }

            if (request.ExpectedOutcomes != null && request.ExpectedOutcomes != theme.ExpectedOutcomes)
            {
                theme.ExpectedOutcomes = request.ExpectedOutcomes.Trim();
                changedFields.Add("ExpectedOutcomes");
                newValues["ExpectedOutcomes"] = theme.ExpectedOutcomes;
            }

            if (request.EstimatedFunding.HasValue && request.EstimatedFunding.Value != theme.EstimatedFunding)
            {
                theme.EstimatedFunding = request.EstimatedFunding.Value;
                changedFields.Add("EstimatedFunding");
                newValues["EstimatedFunding"] = theme.EstimatedFunding;
            }

            if (request.ResearchFieldId.HasValue && request.ResearchFieldId.Value != theme.ResearchFieldId)
            {
                theme.ResearchFieldId = request.ResearchFieldId.Value;
                changedFields.Add("ResearchFieldId");
                newValues["ResearchFieldId"] = theme.ResearchFieldId?.ToString() ?? string.Empty;
            }

            // Handle file uploads (stored in FileStorage table)
            if (request.Image != null && request.Image.Length > 0)
            {
                // Delete old image from FileStorage if exists
                if (oldImageFileId.HasValue)
                {
                    await _fileUploadService.DeleteFileAsync(oldImageFileId.Value.ToString());
                }

                // Upload new image to FileStorage
                var newImageFileId = await _fileUploadService.UploadFileAsync(
                    request.Image,
                    "Theme",
                    theme.Id,
                    "Image",
                    request.UpdatedBy);
                
                changedFields.Add("ImageUrl");
                newValues["ImageUrl"] = newImageFileId;
            }

            if (request.Document != null && request.Document.Length > 0)
            {
                // Delete old document from FileStorage if exists
                if (oldDocumentFileId.HasValue)
                {
                    await _fileUploadService.DeleteFileAsync(oldDocumentFileId.Value.ToString());
                }

                // Upload new document to FileStorage
                var newDocumentFileId = await _fileUploadService.UploadFileAsync(
                    request.Document,
                    "Theme",
                    theme.Id,
                    "Document",
                    request.UpdatedBy);
                
                changedFields.Add("DocumentUrl");
                newValues["DocumentUrl"] = newDocumentFileId;
            }

            // Update timestamp
            theme.UpdatedAt = DateTime.UtcNow;

            // Save the changes
            var updatedTheme = await _themeRepository.UpdateAsync(theme);

            // Get actual user names
            var updater = await _userRepository.GetByIdAsync(request.UpdatedBy);
            var updaterName = updater?.Name ?? "Unknown";

            var submitter = await _userRepository.GetByIdAsync(updatedTheme.SubmittedBy);
            var submitterName = submitter?.Name ?? "Unknown";

            string? approverName = null;
            if (updatedTheme.ApprovedBy.HasValue)
            {
                var approver = await _userRepository.GetByIdAsync(updatedTheme.ApprovedBy.Value);
                approverName = approver?.Name ?? "Unknown";
            }

            // Get research field name if exists
            string? researchFieldName = null;
            if (updatedTheme.ResearchFieldId.HasValue)
            {
                var researchField = await _researchFieldRepository.GetByIdAsync(updatedTheme.ResearchFieldId.Value);
                researchFieldName = researchField?.Name;
            }

            // Publish domain event
            var domainEvent = new ThemeUpdatedEvent(
                updatedTheme.Id,
                request.UpdatedBy,
                updatedTheme.Title,
                updaterName,
                updatedTheme.Description,
                updatedTheme.ExpectedOutcomes,
                updatedTheme.EstimatedFunding,
                updatedTheme.ResearchFieldId,
                researchFieldName,
                null, // ImageUrl is obsolete - file is in FileStorage
                null, // DocumentUrl is obsolete - file is in FileStorage
                updatedTheme.Slug,
                updatedTheme.Status,
                updatedTheme.SubmittedBy,
                submitterName,
                updatedTheme.ApprovedBy,
                approverName,
                changedFields,
                oldValues,
                newValues
            );

            await _eventBus.PublishAsync(domainEvent);

            return updatedTheme;
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return string.Empty;

            // Convert to lowercase and replace spaces with hyphens
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

            // Remove multiple consecutive hyphens
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }

            // Remove leading and trailing hyphens
            slug = slug.Trim('-');

            return slug;
        }
    }
}
