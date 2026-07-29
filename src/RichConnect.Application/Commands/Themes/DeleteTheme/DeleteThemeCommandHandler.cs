using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Files;
using RICHConnect.Backend.Application.Interfaces.Themes;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Commands.Themes.DeleteTheme
{
    public class DeleteThemeCommandHandler : BaseCommandHandler<DeleteThemeCommand, bool>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IResearchFieldRepository _researchFieldRepository;
        private readonly IEventBus _eventBus;
        private readonly IFileUploadService _fileUploadService;
        private readonly IFileReadService _fileReadService;
        private readonly IThemeBusinessRulesService _businessRulesService;

        public DeleteThemeCommandHandler(
            ILogger<DeleteThemeCommandHandler> logger,
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

        protected override async Task<bool> HandleInternal(DeleteThemeCommand request, CancellationToken cancellationToken)
        {
            // Validation is handled by ValidationBehavior and DeleteThemeCommandValidator
            var theme = await _themeRepository.GetByIdAsync(request.ThemeId);
            if (theme == null)
            {
                throw new InvalidOperationException($"Theme with ID {request.ThemeId} not found.");
            }

            // Validate user can delete this theme
            var canDelete = await _businessRulesService.CanUserDeleteThemeAsync(request.ThemeId, request.DeletedBy);
            if (!canDelete.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", canDelete.Errors));
            }

            // Validate theme can be deleted (check dependencies)
            var deletionValidation = await _businessRulesService.ValidateThemeDeletionAsync(request.ThemeId, request.DeletedBy);
            if (!deletionValidation.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", deletionValidation.Errors));
            }

            // Get file IDs from FileStorage for audit
            var imageFileId = await _fileReadService.GetFileIdByEntityAsync("Theme", theme.Id, "Image");
            var documentFileId = await _fileReadService.GetFileIdByEntityAsync("Theme", theme.Id, "Document");

            // Store theme data for audit before deletion
            var themeData = new
            {
                theme.Title,
                theme.Description,
                theme.ExpectedOutcomes,
                theme.EstimatedFunding,
                theme.Status,
                ImageUrl = imageFileId?.ToString(),
                DocumentUrl = documentFileId?.ToString(),
                theme.Slug,
                theme.SubmittedBy,
                theme.ApprovedBy,
                theme.CreatedAt,
                theme.UpdatedAt
            };

            // Soft delete associated files from FileStorage if they exist
            if (imageFileId.HasValue)
            {
                await _fileUploadService.DeleteFileAsync(imageFileId.Value.ToString());
            }

            if (documentFileId.HasValue)
            {
                await _fileUploadService.DeleteFileAsync(documentFileId.Value.ToString());
            }

            // Delete the theme from database
            await _themeRepository.DeleteAsync(request.ThemeId);

            // Get actual user names
            var deleter = await _userRepository.GetByIdAsync(request.DeletedBy);
            var deleterName = deleter?.Name ?? "Unknown";

            var submitter = await _userRepository.GetByIdAsync(theme.SubmittedBy);
            var submitterName = submitter?.Name ?? "Unknown";

            string? approverName = null;
            if (theme.ApprovedBy.HasValue)
            {
                var approver = await _userRepository.GetByIdAsync(theme.ApprovedBy.Value);
                approverName = approver?.Name ?? "Unknown";
            }

            // Get research field name if exists
            string? researchFieldName = null;
            if (theme.ResearchFieldId.HasValue)
            {
                var researchField = await _researchFieldRepository.GetByIdAsync(theme.ResearchFieldId.Value);
                researchFieldName = researchField?.Name;
            }

            // Publish domain event
            var domainEvent = new ThemeDeletedEvent(
                theme.Id,
                request.DeletedBy,
                theme.Title,
                deleterName,
                theme.Description,
                theme.ExpectedOutcomes,
                theme.EstimatedFunding,
                theme.ResearchFieldId,
                researchFieldName,
                null, // ImageUrl is obsolete - file is in FileStorage
                null, // DocumentUrl is obsolete - file is in FileStorage
                theme.Slug,
                theme.Status,
                theme.SubmittedBy,
                submitterName,
                theme.ApprovedBy,
                approverName,
                theme.CreatedAt,
                theme.UpdatedAt
            );

            await _eventBus.PublishAsync(domainEvent);

            return true;
        }
    }
}
