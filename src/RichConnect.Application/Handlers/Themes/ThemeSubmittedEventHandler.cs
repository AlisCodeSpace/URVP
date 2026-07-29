using MediatR;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeSubmitted;
using RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeSubmissionConfirmation;
using RICHConnect.Backend.Application.Interfaces.Search;
using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers.Themes
{
    public class ThemeSubmittedEventHandler : IEventHandler<ThemeSubmittedEvent>
    {
        private readonly IMediator _mediator;
        private readonly ISearchIndexingService _searchIndexingService;
        private readonly ILogger<ThemeSubmittedEventHandler> _logger;

        public ThemeSubmittedEventHandler(
            IMediator mediator,
            ISearchIndexingService searchIndexingService,
            ILogger<ThemeSubmittedEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _searchIndexingService = searchIndexingService ?? throw new ArgumentNullException(nameof(searchIndexingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(ThemeSubmittedEvent domainEvent)
        {
            try
            {
                _logger.LogInformation("Handling ThemeSubmittedEvent for theme: {ThemeId} - {Title}", 
                    domainEvent.ThemeId, domainEvent.ThemeTitle);

                // Send notification to admins for review using CQRS
                await _mediator.Send(new NotifyThemeSubmittedCommand
                {
                    ThemeId = domainEvent.ThemeId,
                    SubmittedByUserId = domainEvent.SubmittedByUserId
                });

                // Send confirmation notification to submitter
                try
                {
                    await _mediator.Send(new NotifyThemeSubmissionConfirmationCommand
                    {
                        ThemeId = domainEvent.ThemeId,
                        SubmittedByUserId = domainEvent.SubmittedByUserId
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send confirmation notification for theme {ThemeId}", domainEvent.ThemeId);
                }

                // Log theme submission for audit
                _logger.LogInformation("Theme submitted successfully: {ThemeId} by user {UserId} - {Title}", 
                    domainEvent.ThemeId, domainEvent.SubmittedByUserId, domainEvent.ThemeTitle);

                // Index theme for search
                try
                {
                    await _searchIndexingService.IndexThemeAsync(domainEvent.ThemeId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to index theme {ThemeId} for search", domainEvent.ThemeId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ThemeSubmittedEvent for theme: {ThemeId}", domainEvent.ThemeId);
                throw;
            }
        }
    }
}

