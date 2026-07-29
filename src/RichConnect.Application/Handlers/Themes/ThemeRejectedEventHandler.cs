using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeRejected;
using RICHConnect.Backend.Application.Interfaces.Search;
using RICHConnect.Backend.Application.Interfaces.Archiving;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.Themes
{
    public class ThemeRejectedEventHandler : IEventHandler<ThemeRejectedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ISearchIndexingService _searchIndexingService;
        private readonly IArchivingService _archivingService;
        private readonly ILogger<ThemeRejectedEventHandler> _logger;

        public ThemeRejectedEventHandler(
            IMediator mediator,
            ISearchIndexingService searchIndexingService,
            IArchivingService archivingService,
            ILogger<ThemeRejectedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _searchIndexingService = searchIndexingService ?? throw new ArgumentNullException(nameof(searchIndexingService));
            _archivingService = archivingService ?? throw new ArgumentNullException(nameof(archivingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(ThemeRejectedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ThemeRejectedEvent for theme: {ThemeId} - {Title}", 
                    domainEvent.ThemeId, domainEvent.ThemeTitle);

                // Send rejection notification to submitter with reason using CQRS
                await _mediator.Send(new NotifyThemeRejectedCommand
                {
                    ThemeId = domainEvent.ThemeId,
                    RejectedByUserId = domainEvent.RejectedByUserId,
                    RejectionReason = domainEvent.RejectionReason
                });

                // Log rejection action for audit
                _logger.LogInformation("Theme rejected: {ThemeId} by admin {AdminId} - {Title}. Reason: {Reason}", 
                    domainEvent.ThemeId, domainEvent.RejectedByUserId, domainEvent.ThemeTitle, domainEvent.RejectionReason);

                // Archive rejected theme for compliance
                try
                {
                    await _archivingService.ArchiveRejectedThemeAsync(domainEvent.ThemeId, domainEvent.RejectedByUserId, domainEvent.RejectionReason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to archive rejected theme {ThemeId}", domainEvent.ThemeId);
                }

                // Remove from search index
                try
                {
                    await _searchIndexingService.RemoveThemeFromIndexAsync(domainEvent.ThemeId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to remove rejected theme {ThemeId} from search index", domainEvent.ThemeId);
                }

                // Note: Notifications to research field specialists can be added later if needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ThemeRejectedEvent for theme: {ThemeId}", domainEvent.ThemeId);
                throw;
            }
        }
    }
}

