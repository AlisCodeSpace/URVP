using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldRejected;
using RICHConnect.Backend.Application.Interfaces.Search;
using RICHConnect.Backend.Application.Interfaces.Archiving;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.ResearchFields
{
    public class ResearchFieldRejectedEventHandler : IEventHandler<ResearchFieldRejectedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ISearchIndexingService _searchIndexingService;
        private readonly IArchivingService _archivingService;
        private readonly ILogger<ResearchFieldRejectedEventHandler> _logger;

        public ResearchFieldRejectedEventHandler(
            IMediator mediator,
            ISearchIndexingService searchIndexingService,
            IArchivingService archivingService,
            ILogger<ResearchFieldRejectedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _searchIndexingService = searchIndexingService ?? throw new ArgumentNullException(nameof(searchIndexingService));
            _archivingService = archivingService ?? throw new ArgumentNullException(nameof(archivingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task HandleAsync(ResearchFieldRejectedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ResearchFieldRejectedEvent for field {FieldId} rejected by {RejectedBy}", 
                    domainEvent.FieldId, domainEvent.RejectedBy);

                // Send rejection notification with reason to submitter using CQRS
                await _mediator.Send(new NotifyResearchFieldRejectedCommand
                {
                    FieldId = domainEvent.FieldId,
                    RejectedByUserId = domainEvent.RejectedBy,
                    RejectionReason = domainEvent.RejectionReason
                });
                
                _logger.LogInformation("Sent rejection notification for research field {FieldId} with reason: {RejectionReason}", 
                    domainEvent.FieldId, domainEvent.RejectionReason);

                // Log rejection action
                _logger.LogInformation("Research field {FieldId} rejected by {RejectedBy} with reason: {RejectionReason}", 
                    domainEvent.FieldId, 
                    domainEvent.RejectedBy, 
                    domainEvent.RejectionReason);

                // Archive rejected research field
                try
                {
                    await _archivingService.ArchiveRejectedResearchFieldAsync(domainEvent.FieldId, domainEvent.RejectedBy, domainEvent.RejectionReason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to archive rejected research field {FieldId}", domainEvent.FieldId);
                }

                // Remove from search index
                try
                {
                    await _searchIndexingService.RemoveResearchFieldFromIndexAsync(domainEvent.FieldId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove rejected research field {FieldId} from search index", domainEvent.FieldId);
                }

                // Note: Rejected fields are not in the catalog, so no removal needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ResearchFieldRejectedEvent for field {FieldId}", domainEvent.FieldId);
                throw;
            }
        }
    }
}

