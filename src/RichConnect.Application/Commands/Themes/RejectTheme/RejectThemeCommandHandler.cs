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

namespace RICHConnect.Backend.Application.Commands.Themes.RejectTheme
{
    public class RejectThemeCommandHandler : BaseCommandHandler<RejectThemeCommand, ResearchTheme>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IResearchFieldRepository _researchFieldRepository;
        private readonly IEventBus _eventBus;
        private readonly IFileReadService _fileReadService;
        private readonly IThemeBusinessRulesService _businessRulesService;

        public RejectThemeCommandHandler(
            ILogger<RejectThemeCommandHandler> logger,
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

        protected override async Task<ResearchTheme> HandleInternal(RejectThemeCommand request, CancellationToken cancellationToken)
        {
            // Validation is handled by ValidationBehavior and RejectThemeCommandValidator
            var theme = await _themeRepository.GetByIdAsync(request.ThemeId);
            if (theme == null)
            {
                throw new InvalidOperationException($"Theme with ID {request.ThemeId} not found.");
            }

            // Validate user can approve themes (which includes rejecting)
            var canReject = await _businessRulesService.CanUserApproveThemesAsync(request.RejectedBy);
            if (!canReject.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", canReject.Errors));
            }

            // Validate theme rejection
            var rejectionValidation = await _businessRulesService.ValidateThemeRejectionAsync(
                request.ThemeId, 
                request.RejectedBy, 
                request.RejectionReason);
            if (!rejectionValidation.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", rejectionValidation.Errors));
            }

            // Validate workflow transition
            var transitionValidation = await _businessRulesService.ValidateThemeWorkflowTransitionAsync(
                request.ThemeId,
                theme.Status,
                ApprovalStatus.Rejected,
                request.RejectedBy);

            if (!transitionValidation.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", transitionValidation.Errors));
            }

            // Update the theme
            theme.Status = ApprovalStatus.Rejected;
            theme.ApprovedBy = request.RejectedBy; // Store who rejected it
            theme.UpdatedAt = DateTime.UtcNow;

            // Save the changes
            var updatedTheme = await _themeRepository.UpdateAsync(theme);

            // Get file ID from FileStorage for event
            var documentFileId = await _fileReadService.GetFileIdByEntityAsync("Theme", updatedTheme.Id, "Document");

            // Get actual user names
            var rejector = await _userRepository.GetByIdAsync(request.RejectedBy);
            var rejectorName = rejector?.Name ?? "Unknown";

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
            var domainEvent = new ThemeRejectedEvent(
                updatedTheme.Id,
                request.RejectedBy,
                updatedTheme.Title,
                rejectorName,
                updatedTheme.SubmittedBy,
                submitterName,
                request.RejectionReason,
                updatedTheme.Description,
                updatedTheme.ExpectedOutcomes,
                updatedTheme.EstimatedFunding,
                updatedTheme.ResearchFieldId,
                researchFieldName,
                documentFileId?.ToString(),
                updatedTheme.Slug
            );

            await _eventBus.PublishAsync(domainEvent);

            return updatedTheme;
        }
    }
}
