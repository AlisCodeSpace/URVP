using RICHConnect.Backend.Application.Common;
using RICHConnect.Backend.Application.Interfaces.Themes;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Commands.Themes.UnpublishTheme
{
    public class UnpublishThemeCommandHandler : BaseCommandHandler<UnpublishThemeCommand, ResearchTheme>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly IThemeBusinessRulesService _businessRulesService;

        public UnpublishThemeCommandHandler(
            ILogger<UnpublishThemeCommandHandler> logger,
            AppDbContext context,
            IThemeRepository themeRepository,
            IThemeBusinessRulesService businessRulesService)
            : base(logger, context)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _businessRulesService = businessRulesService ?? throw new ArgumentNullException(nameof(businessRulesService));
        }

        protected override async Task<ResearchTheme> HandleInternal(UnpublishThemeCommand request, CancellationToken cancellationToken)
        {
            var theme = await _themeRepository.GetByIdAsync(request.ThemeId);
            if (theme == null)
            {
                throw new InvalidOperationException($"Theme with ID {request.ThemeId} not found.");
            }

            // Validate user can unpublish themes (must be admin)
            var canUnpublish = await _businessRulesService.CanUserApproveThemesAsync(request.UnpublishedBy);
            if (!canUnpublish.IsValid)
            {
                throw new InvalidOperationException(string.Join("; ", canUnpublish.Errors));
            }

            // Idempotency: if already unpublished, return existing theme
            if (!theme.IsPublished)
            {
                _logger.LogInformation(
                    "UnpublishTheme called for already unpublished theme {ThemeId} by user {UserId}. Returning existing theme.",
                    request.ThemeId,
                    request.UnpublishedBy);
                return theme;
            }

            // Update the theme
            theme.IsPublished = false;
            theme.UpdatedAt = DateTime.UtcNow;

            // Save the changes
            var updatedTheme = await _themeRepository.UpdateAsync(theme);

            _logger.LogInformation("Successfully unpublished theme: {ThemeId} - {Title}", updatedTheme.Id, updatedTheme.Title);

            return updatedTheme;
        }
    }
}
