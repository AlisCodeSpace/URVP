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

namespace RICHConnect.Backend.Application.Commands.Themes.ApproveTheme
{
    public class ApproveThemeCommandHandler : BaseCommandHandler<ApproveThemeCommand, ResearchTheme>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IResearchFieldRepository _researchFieldRepository;
        private readonly IEventBus _eventBus;
        private readonly IFileReadService _fileReadService;
        private readonly IThemeBusinessRulesService _businessRulesService;

        public ApproveThemeCommandHandler(
            ILogger<ApproveThemeCommandHandler> logger,
            AppDbContext context,
            IThemeRepository themeRepository,
            IUserRepository userRepository,
            IResearchFieldRepository researchFieldRepository,
            IEventBus eventBus,
            IFileReadService fileReadService,
            IThemeBusinessRulesService businessRulesService)
            : base(logger, context)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _researchFieldRepository = researchFieldRepository ?? throw new ArgumentNullException(nameof(researchFieldRepository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _fileReadService = fileReadService ?? throw new ArgumentNullException(nameof(fileReadService));
            _businessRulesService = businessRulesService ?? throw new ArgumentNullException(nameof(businessRulesService));
        }

        protected override async Task<ResearchTheme> HandleInternal(ApproveThemeCommand request, CancellationToken cancellationToken)
        {
            // Validation is handled by ValidationBehavior and ApproveThemeCommandValidator
            var theme = await _themeRepository.GetByIdAsync(request.ThemeId);
            if (theme == null)
            {
                throw new InvalidOperationException($"Theme with ID {request.ThemeId} not found.");
            }

            // Validate user can approve themes
            var canApprove = await _businessRulesService.CanUserApproveThemesAsync(request.ApprovedBy);
            if (!canApprove.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", canApprove.Errors));
            }

            // Idempotency: if the theme is already approved, treat this as a no-op success.
            // This prevents "double click" scenarios from showing an error even though the first request succeeded,
            // and avoids publishing duplicate approval events.
            if (theme.Status == ApprovalStatus.Approved)
            {
                _logger.LogInformation(
                    "ApproveTheme called for already approved theme {ThemeId} by user {UserId}. Returning existing theme.",
                    request.ThemeId,
                    request.ApprovedBy);
                return theme;
            }

            // Validate theme approval
            var approvalValidation = await _businessRulesService.ValidateThemeApprovalAsync(request.ThemeId, request.ApprovedBy);
            if (!approvalValidation.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", approvalValidation.Errors));
            }

            // Validate workflow transition
            var transitionValidation = await _businessRulesService.ValidateThemeWorkflowTransitionAsync(
                request.ThemeId,
                theme.Status,
                ApprovalStatus.Approved,
                request.ApprovedBy);

            if (!transitionValidation.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", transitionValidation.Errors));
            }

            // Update the theme
            theme.Status = ApprovalStatus.Approved;
            theme.ApprovedBy = request.ApprovedBy;
            theme.UpdatedAt = DateTime.UtcNow;

            // Save the changes
            var updatedTheme = await _themeRepository.UpdateAsync(theme);

            // Get file IDs from FileStorage for event
            var imageFileId = await _fileReadService.GetFileIdByEntityAsync("Theme", updatedTheme.Id, "Image");
            var documentFileId = await _fileReadService.GetFileIdByEntityAsync("Theme", updatedTheme.Id, "Document");

            // Get actual user names
            var approver = await _userRepository.GetByIdAsync(request.ApprovedBy);
            var approverName = approver?.Name ?? "Unknown";

            var submitter = await _userRepository.GetByIdAsync(updatedTheme.SubmittedBy);
            var submitterName = submitter?.Name ?? "Unknown";

            // Get research field name if exists
            string? researchFieldName = null;
            if (updatedTheme.ResearchFieldId.HasValue)
            {
                var researchField = await _researchFieldRepository.GetByIdAsync(updatedTheme.ResearchFieldId.Value);
                researchFieldName = researchField?.Name;
            }

            // Publish domain event
            var domainEvent = new ThemeApprovedEvent(
                updatedTheme.Id,
                request.ApprovedBy,
                updatedTheme.Title,
                approverName,
                updatedTheme.SubmittedBy,
                submitterName,
                updatedTheme.Description,
                updatedTheme.ExpectedOutcomes,
                updatedTheme.EstimatedFunding,
                updatedTheme.ResearchFieldId,
                researchFieldName,
                imageFileId?.ToString(),
                documentFileId?.ToString(),
                updatedTheme.Slug
            );

            await _eventBus.PublishAsync(domainEvent);

            return updatedTheme;
        }
    }
}
