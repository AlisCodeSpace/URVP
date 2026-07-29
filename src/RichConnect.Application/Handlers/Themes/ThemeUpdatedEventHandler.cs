using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Interfaces.Search;

namespace RICHConnect.Backend.Application.Handlers.Themes
{
    public class ThemeUpdatedEventHandler : IEventHandler<ThemeUpdatedEvent>
    {
        private readonly ISearchIndexingService _searchIndexingService;
        private readonly ILogger<ThemeUpdatedEventHandler> _logger;

        public ThemeUpdatedEventHandler(
            ISearchIndexingService searchIndexingService,
            ILogger<ThemeUpdatedEventHandler> logger)
        {
            _searchIndexingService = searchIndexingService ?? throw new ArgumentNullException(nameof(searchIndexingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(ThemeUpdatedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ThemeUpdatedEvent for theme: {ThemeId} - {Title}", 
                    domainEvent.ThemeId, domainEvent.ThemeTitle);

                // Log theme update for audit
                _logger.LogInformation("Theme updated: {ThemeId} by user {UserId} - {Title}. Changed fields: {ChangedFields}", 
                    domainEvent.ThemeId, domainEvent.UpdatedByUserId, domainEvent.ThemeTitle, 
                    string.Join(", ", domainEvent.ChangedFields));

                // Update search index with updated theme
                try
                {
                    await _searchIndexingService.UpdateThemeIndexAsync(domainEvent.ThemeId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update search index for theme {ThemeId}", domainEvent.ThemeId);
                }

                // Note: Challenges automatically see updated theme data via their ResearchThemeId foreign key
                // No additional notification to challenges needed

                // Note: Additional notifications to stakeholders can be added later if needed
                // This would require creating specific notification commands and handlers
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ThemeUpdatedEvent for theme: {ThemeId}", domainEvent.ThemeId);
                throw;
            }
        }
    }
}

