using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Queries.Themes.GetUserThemes
{
    public class GetUserThemesQueryHandler : IRequestHandler<GetUserThemesQuery, List<ResearchTheme>>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly ILogger<GetUserThemesQueryHandler> _logger;

        public GetUserThemesQueryHandler(
            IThemeRepository themeRepository,
            ILogger<GetUserThemesQueryHandler> logger)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ResearchTheme>> Handle(GetUserThemesQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling GetUserThemesQuery for user: {UserId}", query.UserId);

                var themes = await _themeRepository.GetByUserWithIncludesAsync(query.UserId);

                // Apply status filter if provided
                if (query.Status.HasValue)
                {
                    themes = themes.Where(t => t.Status == query.Status.Value).ToList();
                }

                // Apply additional filters if needed
                if (!query.IncludeInactive)
                {
                    // Filter out inactive themes if needed (this would depend on your business logic)
                    // For now, we'll assume all themes are active unless they have a specific flag
                }

                _logger.LogInformation("Successfully retrieved {Count} themes for user: {UserId}", 
                    themes.Count, query.UserId);
                return themes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetUserThemesQuery for user: {UserId}", query.UserId);
                throw;
            }
        }
    }
}

