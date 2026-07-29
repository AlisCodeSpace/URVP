using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Queries.Themes.GetAllThemes
{
    public class GetAllThemesQueryHandler : IRequestHandler<GetAllThemesQuery, List<ResearchTheme>>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly ILogger<GetAllThemesQueryHandler> _logger;

        public GetAllThemesQueryHandler(
            IThemeRepository themeRepository,
            ILogger<GetAllThemesQueryHandler> logger)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ResearchTheme>> Handle(GetAllThemesQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling GetAllThemesQuery with filters - Status: {Status}, UserId: {UserId}, ResearchFieldId: {ResearchFieldId}", 
                    query.Status, query.UserId, query.ResearchFieldId);

                List<ResearchTheme> themes;

                // Start with all themes
                themes = await _themeRepository.GetAllWithIncludesAsync();

                // Apply status filter
                if (query.Status.HasValue)
                {
                    themes = themes.Where(t => t.Status == query.Status.Value).ToList();
                }

                // Apply user filter
                if (query.UserId.HasValue)
                {
                    themes = themes.Where(t => t.SubmittedBy == query.UserId.Value).ToList();
                }

                // Apply research field filter
                if (query.ResearchFieldId.HasValue)
                {
                    themes = themes.Where(t => t.ResearchFieldId == query.ResearchFieldId.Value).ToList();
                }

                // Apply search term filter
                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    var searchTerm = query.SearchTerm.ToLowerInvariant();
                    themes = themes.Where(t => t.Title.ToLowerInvariant().Contains(searchTerm)).ToList();
                }

                // Apply date range filter
                if (query.FromDate.HasValue)
                {
                    themes = themes.Where(t => t.CreatedAt >= query.FromDate.Value).ToList();
                }

                if (query.ToDate.HasValue)
                {
                    themes = themes.Where(t => t.CreatedAt <= query.ToDate.Value).ToList();
                }

                // Apply inactive filter
                if (!query.IncludeInactive)
                {
                    // Filter out inactive themes if needed (this would depend on your business logic)
                    // For now, we'll assume all themes are active unless they have a specific flag
                }

                _logger.LogInformation("Successfully retrieved {Count} themes with applied filters", themes.Count);
                return themes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetAllThemesQuery");
                throw;
            }
        }
    }
}
