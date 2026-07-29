using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldApproved;
using RICHConnect.Backend.Application.Interfaces.Search;
using RICHConnect.Backend.Application.Interfaces.ResearchFields;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.ResearchFields
{
    public class ResearchFieldApprovedEventHandler : IEventHandler<ResearchFieldApprovedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ISearchIndexingService _searchIndexingService;
        private readonly IResearchFieldCatalogService _catalogService;
        private readonly ILogger<ResearchFieldApprovedEventHandler> _logger;

        public ResearchFieldApprovedEventHandler(
            IMediator mediator,
            ISearchIndexingService searchIndexingService,
            IResearchFieldCatalogService catalogService,
            ILogger<ResearchFieldApprovedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _searchIndexingService = searchIndexingService ?? throw new ArgumentNullException(nameof(searchIndexingService));
            _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task HandleAsync(ResearchFieldApprovedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ResearchFieldApprovedEvent for field {FieldId} approved by {ApprovedBy}", 
                    domainEvent.FieldId, domainEvent.ApprovedBy);

                // Send approval notification to submitter using CQRS
                await _mediator.Send(new NotifyResearchFieldApprovedCommand
                {
                    FieldId = domainEvent.FieldId,
                    ApprovedByUserId = domainEvent.ApprovedBy
                });
                
                _logger.LogInformation("Sent approval notification for research field {FieldId}", domainEvent.FieldId);

                // Log approval action
                _logger.LogInformation("Research field {FieldId} approved by {ApprovedBy} at {ApprovedAt}", 
                    domainEvent.FieldId, 
                    domainEvent.ApprovedBy, 
                    domainEvent.ApprovedAt);

                // Note: Field is automatically available for challenges via the ResearchFieldId foreign key

                // Update search index
                try
                {
                    await _searchIndexingService.IndexResearchFieldAsync(domainEvent.FieldId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to index research field {FieldId} for search", domainEvent.FieldId);
                }

                // Add to catalog
                try
                {
                    await _catalogService.AddToCatalogAsync(domainEvent.FieldId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add research field {FieldId} to catalog", domainEvent.FieldId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ResearchFieldApprovedEvent for field {FieldId}", domainEvent.FieldId);
                throw;
            }
        }
    }
}

