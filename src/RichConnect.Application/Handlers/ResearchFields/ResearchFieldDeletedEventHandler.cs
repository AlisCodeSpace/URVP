using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Interfaces.Search;
using RICHConnect.Backend.Application.Interfaces.Archiving;
using RICHConnect.Backend.Application.Interfaces.ResearchFields;

namespace RICHConnect.Backend.Application.Handlers.ResearchFields
{
    public class ResearchFieldDeletedEventHandler : IEventHandler<ResearchFieldDeletedEvent>
    {
        private readonly ISearchIndexingService _searchIndexingService;
        private readonly IArchivingService _archivingService;
        private readonly IResearchFieldCatalogService _catalogService;
        private readonly ILogger<ResearchFieldDeletedEventHandler> _logger;

        public ResearchFieldDeletedEventHandler(
            ISearchIndexingService searchIndexingService,
            IArchivingService archivingService,
            IResearchFieldCatalogService catalogService,
            ILogger<ResearchFieldDeletedEventHandler> logger)
        {
            _searchIndexingService = searchIndexingService ?? throw new ArgumentNullException(nameof(searchIndexingService));
            _archivingService = archivingService ?? throw new ArgumentNullException(nameof(archivingService));
            _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task HandleAsync(ResearchFieldDeletedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ResearchFieldDeletedEvent for field {FieldId} deleted by {DeletedBy}", 
                    domainEvent.FieldId, domainEvent.DeletedBy);

                // Log field deletion
                _logger.LogInformation("Research field {FieldId} deleted by {DeletedBy}", 
                    domainEvent.FieldId, 
                    domainEvent.DeletedBy);

                // Archive deleted research field
                try
                {
                    await _archivingService.ArchiveDeletedResearchFieldAsync(domainEvent.FieldId, domainEvent.DeletedBy, "Research field deletion");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to archive deleted research field {FieldId}", domainEvent.FieldId);
                }

                // Remove from search index
                try
                {
                    await _searchIndexingService.RemoveResearchFieldFromIndexAsync(domainEvent.FieldId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove research field {FieldId} from search index", domainEvent.FieldId);
                }

                // Remove from catalog
                try
                {
                    await _catalogService.RemoveFromCatalogAsync(domainEvent.FieldId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove research field {FieldId} from catalog", domainEvent.FieldId);
                }

                // Note: Challenges and themes with dependencies should have been prevented by business rules
                // Note: Additional notifications can be added later if needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ResearchFieldDeletedEvent for field {FieldId}", domainEvent.FieldId);
                throw;
            }
        }
    }
}

