using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Queries.Themes.GetThemeById
{
    public class GetThemeByIdQueryHandler : IRequestHandler<GetThemeByIdQuery, ResearchTheme?>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly ILogger<GetThemeByIdQueryHandler> _logger;

        public GetThemeByIdQueryHandler(
            IThemeRepository themeRepository,
            ILogger<GetThemeByIdQueryHandler> logger)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResearchTheme?> Handle(GetThemeByIdQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling GetThemeByIdQuery for theme: {ThemeId}", query.ThemeId);

                var theme = await _themeRepository.GetByIdWithIncludesAsync(query.ThemeId);
                
                if (theme == null)
                {
                    _logger.LogWarning("Theme not found: {ThemeId}", query.ThemeId);
                    return null;
                }

                _logger.LogInformation("Successfully retrieved theme: {ThemeId} - {Title}", theme.Id, theme.Title);
                return theme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetThemeByIdQuery for theme: {ThemeId}", query.ThemeId);
                throw;
            }
        }
    }
}
