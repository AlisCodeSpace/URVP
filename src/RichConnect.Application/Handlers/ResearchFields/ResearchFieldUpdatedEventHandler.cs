using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Interfaces.Search;
using RICHConnect.Backend.Application.Interfaces.ResearchFields;

namespace RICHConnect.Backend.Application.Handlers.ResearchFields
{
    public class ResearchFieldUpdatedEventHandler : IEventHandler<ResearchFieldUpdatedEvent>
    {
        private readonly ISearchIndexingService _searchIndexingService;
        private readonly IResearchFieldCatalogService _catalogService;
        private readonly ILogger<ResearchFieldUpdatedEventHandler> _logger;

        public ResearchFieldUpdatedEventHandler(
            ISearchIndexingService searchIndexingService,
            IResearchFieldCatalogService catalogService,
            ILogger<ResearchFieldUpdatedEventHandler> logger)
        {
            _searchIndexingService = searchIndexingService ?? throw new ArgumentNullException(nameof(searchIndexingService));
            _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task HandleAsync(ResearchFieldUpdatedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ResearchFieldUpdatedEvent for field {FieldId} updated by {UpdatedBy}", 
                    domainEvent.FieldId, domainEvent.UpdatedBy);

                // Log field update with details about changes
                _logger.LogInformation("Research field {FieldId} updated by {UpdatedBy}. Changes: {Changes}", 
                    domainEvent.FieldId, 
                    domainEvent.UpdatedBy, 
                    string.Join(", ", domainEvent.Changes.Keys));

                // Update search index
                try
                {
                    await _searchIndexingService.UpdateResearchFieldIndexAsync(domainEvent.FieldId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update search index for research field {FieldId}", domainEvent.FieldId);
                }

                // Update catalog
                try
                {
                    await _catalogService.UpdateCatalogEntryAsync(domainEvent.FieldId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update catalog for research field {FieldId}", domainEvent.FieldId);
                }

                // Note: Challenges and themes automatically see updated field data via their ResearchFieldId foreign key
                // Note: Additional notifications can be added later if needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ResearchFieldUpdatedEvent for field {FieldId}", domainEvent.FieldId);
                throw;
            }
        }
    }
}

