using MediatR;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Themes.Interfaces;
using RICHConnect.Backend.Domain.Entities.Themes;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Queries.Themes.GetThemesByStatus
{
    public class GetThemesByStatusQueryHandler : IRequestHandler<GetThemesByStatusQuery, List<ResearchTheme>>
    {
        private readonly IThemeRepository _themeRepository;
        private readonly ILogger<GetThemesByStatusQueryHandler> _logger;

        public GetThemesByStatusQueryHandler(
            IThemeRepository themeRepository,
            ILogger<GetThemesByStatusQueryHandler> logger)
        {
            _themeRepository = themeRepository ?? throw new ArgumentNullException(nameof(themeRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ResearchTheme>> Handle(GetThemesByStatusQuery query, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Handling GetThemesByStatusQuery for status: {Status}", query.Status);

                List<ResearchTheme> themes;

                if (query.UserId.HasValue)
                {
                    // Get themes by status and user
                    themes = await _themeRepository.GetByUserWithIncludesAsync(query.UserId.Value);
                    themes = themes.Where(t => t.Status == query.Status).ToList();
                }
                else
                {
                    // Get themes by status only
                    themes = await _themeRepository.GetByStatusWithIncludesAsync(query.Status);
                }

                // Apply additional filters if needed
                if (!query.IncludeInactive)
                {
                    // Filter out inactive themes if needed (this would depend on your business logic)
                    // For now, we'll assume all themes are active unless they have a specific flag
                }

                // Filter by published status if requested (for public endpoints)
                if (query.OnlyPublished && query.Status == ApprovalStatus.Approved)
                {
                    themes = themes.Where(t => t.IsPublished).ToList();
                }

                _logger.LogInformation("Successfully retrieved {Count} themes with status: {Status}", 
                    themes.Count, query.Status);
                return themes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling GetThemesByStatusQuery for status: {Status}", query.Status);
                throw;
            }
        }
    }
}
