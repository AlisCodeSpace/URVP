using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Themes;
using RICHConnect.Backend.Application.Interfaces.Themes;

namespace RICHConnect.Backend.Application.Commands.Themes.SubmitTheme
{
    public class SubmitThemeCommandHandler : BaseCommandHandler<SubmitThemeCommand, ResearchTheme>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IResearchFieldRepository _researchFieldRepository;
        private readonly IEventBus _eventBus;
        private readonly IFileUploadService _fileUploadService;
        private readonly IThemeBusinessRulesService _businessRulesService;

        public SubmitThemeCommandHandler(
            ILogger<SubmitThemeCommandHandler> logger,
            AppDbContext context,
            IThemeRepository themeRepository,
            IUserRepository userRepository,
            IResearchFieldRepository researchFieldRepository,
            IEventBus eventBus,
            IFileUploadService fileUploadService,
            IThemeBusinessRulesService businessRulesService)
            : base(logger, context)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _researchFieldRepository = researchFieldRepository ?? throw new ArgumentNullException(nameof(researchFieldRepository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));
            _businessRulesService = businessRulesService ?? throw new ArgumentNullException(nameof(businessRulesService));
        }

        protected override async Task<ResearchTheme> HandleInternal(SubmitThemeCommand request, CancellationToken cancellationToken)
        {
            // Validation is handled by ValidationBehavior and SubmitThemeCommandValidator

            // Validate user can submit themes and hasn't reached submission limit
            if (!request.IsAdminCreated)
            {
                var canSubmit = await _businessRulesService.CanUserSubmitThemeAsync(request.SubmittedBy);
                if (!canSubmit.IsValid)
                {
                    throw new InvalidOperationException(string.Join("; ", canSubmit.Errors));
                }
            }

            // Validate title uniqueness
            var titleValidation = await _businessRulesService.ValidateTitleUniquenessAsync(request.Title);
            if (!titleValidation.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", titleValidation.Errors));
            }

            // Validate content quality
            var contentValidation = await _businessRulesService.ValidateThemeContentQualityAsync(
                request.Title, 
                request.Description, 
                request.ExpectedOutcomes);
            if (!contentValidation.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", contentValidation.Errors));
            }

            // Validate research field assignment if provided
            if (request.ResearchFieldId.HasValue)
            {
                var fieldValidation = await _businessRulesService.ValidateResearchFieldAssignmentAsync(request.ResearchFieldId);
                if (!fieldValidation.IsValid)
                {
                    throw new InvalidOperationException(string.Join("; ", fieldValidation.Errors));
                }
            }

            // Validate estimated funding
            var fundingValidation = await _businessRulesService.ValidateEstimatedFundingAsync(
                request.EstimatedFunding);
            if (!fundingValidation.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", fundingValidation.Errors));
            }

            // Generate slug from title
            var slug = GenerateSlug(request.Title);

            // Create the theme (without files - files stored in FileStorage table)
            var theme = new ResearchTheme
            {
                Title = request.Title.Trim(),
                Slug = slug,
                Description = request.Description?.Trim(),
                ExpectedOutcomes = request.ExpectedOutcomes?.Trim(),
                EstimatedFunding = request.EstimatedFunding,
                ResearchFieldId = request.ResearchFieldId,
                // Note: ImageUrl and DocumentUrl are obsolete - files stored in FileStorage table
                Status = request.IsAdminCreated ? ApprovalStatus.Approved : ApprovalStatus.Pending,
                SubmittedBy = request.SubmittedBy,
                ApprovedBy = request.IsAdminCreated ? request.SubmittedBy : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save the theme first to get the ID
            var createdTheme = await _themeRepository.CreateAsync(theme);

            // Upload files to FileStorage after theme is created
            // Handle multiple images
            if (request.Images != null && request.Images.Any())
            {
                await _fileUploadService.UploadMultipleFilesAsync(
                    request.Images,
                    "Theme",
                    createdTheme.Id,
                    "Image",
                    request.SubmittedBy);
            }
            else if (request.Image != null && request.Image.Length > 0)
            {
                // Backwards compatibility: single image upload
                await _fileUploadService.UploadFileAsync(
                    request.Image,
                    "Theme",
                    createdTheme.Id,
                    "Image",
                    request.SubmittedBy);
            }

            // Handle multiple documents
            if (request.Documents != null && request.Documents.Any())
            {
                await _fileUploadService.UploadMultipleFilesAsync(
                    request.Documents,
                    "Theme",
                    createdTheme.Id,
                    "Document",
                    request.SubmittedBy);
            }
            else if (request.Document != null && request.Document.Length > 0)
            {
                // Backwards compatibility: single document upload
                await _fileUploadService.UploadFileAsync(
                    request.Document,
                    "Theme",
                    createdTheme.Id,
                    "Document",
                    request.SubmittedBy);
            }

            // Get actual user name
            var submitter = await _userRepository.GetByIdAsync(createdTheme.SubmittedBy);
            var submitterName = submitter?.Name ?? "Unknown";

            // Get research field name if exists
            string? researchFieldName = null;
            if (createdTheme.ResearchFieldId.HasValue)
            {
                var researchField = await _researchFieldRepository.GetByIdAsync(createdTheme.ResearchFieldId.Value);
                researchFieldName = researchField?.Name;
            }

            // Publish domain event
            var domainEvent = new ThemeSubmittedEvent(
                createdTheme.Id,
                createdTheme.SubmittedBy,
                createdTheme.Title,
                submitterName,
                createdTheme.Description,
                createdTheme.ExpectedOutcomes,
                createdTheme.EstimatedFunding,
                createdTheme.ResearchFieldId,
                researchFieldName,
                null, // DocumentUrl is obsolete - file is in FileStorage
                createdTheme.Slug
            );

            await _eventBus.PublishAsync(domainEvent);

            return createdTheme;
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
