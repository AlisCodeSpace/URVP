using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Themes;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Commands.Themes.PublishTheme
{
    public class PublishThemeCommandHandler : BaseCommandHandler<PublishThemeCommand, ResearchTheme>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IThemeBusinessRulesService _businessRulesService;

        public PublishThemeCommandHandler(
            ILogger<PublishThemeCommandHandler> logger,
            AppDbContext context,
            IThemeRepository themeRepository,
            IThemeBusinessRulesService businessRulesService)
            : base(logger, context)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _businessRulesService = businessRulesService ?? throw new ArgumentNullException(nameof(businessRulesService));
        }

        protected override async Task<ResearchTheme> HandleInternal(PublishThemeCommand request, CancellationToken cancellationToken)
        {
            var theme = await _themeRepository.GetByIdAsync(request.ThemeId);
            if (theme == null)
            {
                throw new InvalidOperationException($"Theme with ID {request.ThemeId} not found.");
            }

            // Validate user can publish themes (must be admin)
            var canPublish = await _businessRulesService.CanUserApproveThemesAsync(request.PublishedBy);
            if (!canPublish.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", canPublish.Errors));
            }

            // Only approved themes can be published
            if (theme.Status != ApprovalStatus.Approved)
            {
                throw new InvalidOperationException($"Theme must be approved before it can be published. Current status: {theme.Status}");
            }

            // Idempotency: if already published, return existing theme
            if (theme.IsPublished)
            {
                _logger.LogInformation(
                    "PublishTheme called for already published theme {ThemeId} by user {UserId}. Returning existing theme.",
                    request.ThemeId,
                    request.PublishedBy);
                return theme;
            }

            // Update the theme
            theme.IsPublished = true;
            theme.UpdatedAt = DateTime.UtcNow;

            // Save the changes
            var updatedTheme = await _themeRepository.UpdateAsync(theme);

            _logger.LogInformation("Successfully published theme: {ThemeId} - {Title}", updatedTheme.Id, updatedTheme.Title);

            return updatedTheme;
        }
    }
}
