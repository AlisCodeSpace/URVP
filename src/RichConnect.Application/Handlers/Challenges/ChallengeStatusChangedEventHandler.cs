using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers
{
    /// <summary>
    /// Event handler for ChallengeStatusChangedEvent
    /// </summary>
    public class ChallengeStatusChangedEventHandler : IEventHandler<ChallengeStatusChangedEvent>
    {
        private readonly ILogger<ChallengeStatusChangedEventHandler> _logger;

        public ChallengeStatusChangedEventHandler(ILogger<ChallengeStatusChangedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(ChallengeStatusChangedEvent domainEvent)
        {
            _logger.LogInformation("Handling ChallengeStatusChangedEvent for challenge {ChallengeId}: {PreviousStatus} → {NewStatus}", 
                domainEvent.ChallengeId, domainEvent.PreviousStatus, domainEvent.NewStatus);

            try
            {
                // Log the status change for audit purposes
                _logger.LogInformation("Challenge {ChallengeId} status changed from {PreviousStatus} to {NewStatus} by {ChangedBy} at {OccurredAt}. Reason: {Reason}", 
                    domainEvent.ChallengeId, 
                    domainEvent.PreviousStatus, 
                    domainEvent.NewStatus, 
                    domainEvent.ChangedByName,
                    domainEvent.OccurredAt,
                    domainEvent.Reason ?? "No reason provided");

                // Future: Could add additional side effects here such as:
                // - Update analytics
                // - Send notifications for specific status changes
                // - Trigger workflows based on status transitions
                // - Update external systems

                _logger.LogInformation("Successfully processed ChallengeStatusChangedEvent for challenge {ChallengeId}", 
                    domainEvent.ChallengeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ChallengeStatusChangedEvent for challenge {ChallengeId}", 
                    domainEvent.ChallengeId);
                throw;
            }
            
            return Task.CompletedTask;
        }
    }
}
