using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeApproved;
using RICHConnect.Backend.Application.Interfaces.Search;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.Themes
{
    public class ThemeApprovedEventHandler : IEventHandler<ThemeApprovedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ISearchIndexingService _searchIndexingService;
        private readonly ILogger<ThemeApprovedEventHandler> _logger;

        public ThemeApprovedEventHandler(
            IMediator mediator,
            ISearchIndexingService searchIndexingService,
            ILogger<ThemeApprovedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _searchIndexingService = searchIndexingService ?? throw new ArgumentNullException(nameof(searchIndexingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(ThemeApprovedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ThemeApprovedEvent for theme: {ThemeId} - {Title}", 
                    domainEvent.ThemeId, domainEvent.ThemeTitle);

                // Send approval notification to submitter using CQRS
                await _mediator.Send(new NotifyThemeApprovedCommand
                {
                    ThemeId = domainEvent.ThemeId,
                    ApprovedByUserId = domainEvent.ApprovedByUserId
                });

                // Log approval action for audit
                _logger.LogInformation("Theme approved successfully: {ThemeId} by admin {AdminId} - {Title}", 
                    domainEvent.ThemeId, domainEvent.ApprovedByUserId, domainEvent.ThemeTitle);

                // Note: Theme is already available for challenges via the ResearchThemeId foreign key in Challenges table
                // No additional action needed for challenge system integration

                // Update search index with approved theme
                try
                {
                    await _searchIndexingService.UpdateThemeIndexAsync(domainEvent.ThemeId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update search index for approved theme {ThemeId}", domainEvent.ThemeId);
                }

                // Note: Notifications to research field specialists can be added later if needed
                // This would require a new notification command and handler
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ThemeApprovedEvent for theme: {ThemeId}", domainEvent.ThemeId);
                throw;
            }
        }
    }
}

