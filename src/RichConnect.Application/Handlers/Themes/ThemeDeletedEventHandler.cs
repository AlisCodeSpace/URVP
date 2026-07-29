using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Interfaces.Search;
using RICHConnect.Backend.Application.Interfaces.Archiving;

namespace RICHConnect.Backend.Application.Handlers.Themes
{
    public class ThemeDeletedEventHandler : IEventHandler<ThemeDeletedEvent>
    {
        private readonly ISearchIndexingService _searchIndexingService;
        private readonly IArchivingService _archivingService;
        private readonly ILogger<ThemeDeletedEventHandler> _logger;

        public ThemeDeletedEventHandler(
            ISearchIndexingService searchIndexingService,
            IArchivingService archivingService,
            ILogger<ThemeDeletedEventHandler> logger)
        {
            _searchIndexingService = searchIndexingService ?? throw new ArgumentNullException(nameof(searchIndexingService));
            _archivingService = archivingService ?? throw new ArgumentNullException(nameof(archivingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(ThemeDeletedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ThemeDeletedEvent for theme: {ThemeId} - {Title}", 
                    domainEvent.ThemeId, domainEvent.ThemeTitle);

                // Log theme deletion for audit
                _logger.LogInformation("Theme deleted: {ThemeId} by user {UserId} - {Title}. Status was: {Status}", 
                    domainEvent.ThemeId, domainEvent.DeletedByUserId, domainEvent.ThemeTitle, domainEvent.Status);

                // Archive theme data for compliance/audit purposes
                try
                {
                    await _archivingService.ArchiveDeletedThemeAsync(domainEvent.ThemeId, domainEvent.DeletedByUserId, $"Theme deletion - Status: {domainEvent.Status}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to archive deleted theme {ThemeId}", domainEvent.ThemeId);
                }

                // Remove from search index
                try
                {
                    await _searchIndexingService.RemoveThemeFromIndexAsync(domainEvent.ThemeId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove deleted theme {ThemeId} from search index", domainEvent.ThemeId);
                }

                // Note: Challenges with dependencies should have been prevented by business rules
                // Note: Additional notifications to stakeholders can be added later if needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ThemeDeletedEvent for theme: {ThemeId}", domainEvent.ThemeId);
                throw;
            }
        }
    }
}

