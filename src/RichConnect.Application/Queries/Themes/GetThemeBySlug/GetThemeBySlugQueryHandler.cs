using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Queries.Themes.GetThemeBySlug
{
    public class GetThemeBySlugQueryHandler : IRequestHandler<GetThemeBySlugQuery, ResearchTheme?>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly ILogger<GetThemeBySlugQueryHandler> _logger;

        public GetThemeBySlugQueryHandler(
            IThemeRepository themeRepository,
            ILogger<GetThemeBySlugQueryHandler> logger)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResearchTheme?> Handle(GetThemeBySlugQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling GetThemeBySlugQuery for slug: {Slug}", query.Slug);

                var theme = await _themeRepository.GetBySlugWithIncludesAsync(query.Slug);
                
                if (theme == null)
                {
                    _logger.LogWarning("Theme not found for slug: {Slug}", query.Slug);
                    return null;
                }

                _logger.LogInformation("Successfully retrieved theme: {ThemeId} - {Title} by slug: {Slug}", 
                    theme.Id, theme.Title, query.Slug);
                return theme;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetThemeBySlugQuery for slug: {Slug}", query.Slug);
                throw;
            }
        }
    }
}
