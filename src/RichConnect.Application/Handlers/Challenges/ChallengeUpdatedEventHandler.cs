using RICHConnect.Backend.Domain.Events;

namespace RICHConnect.Backend.Application.Handlers
{
    /// <summary>
    /// Event handler for ChallengeUpdatedEvent
    /// </summary>
    public class ChallengeUpdatedEventHandler : IEventHandler<ChallengeUpdatedEvent>
    {
        private readonly ILogger<ChallengeUpdatedEventHandler> _logger;

        public ChallengeUpdatedEventHandler(ILogger<ChallengeUpdatedEventHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(ChallengeUpdatedEvent domainEvent)
        {
            _logger.LogInformation("Handling ChallengeUpdatedEvent for challenge {ChallengeId}", 
                domainEvent.ChallengeId);

            try
            {
                // Log the update for audit purposes
                var changedFieldsText = string.Join(", ", domainEvent.ChangedFields);
                _logger.LogInformation("Challenge {ChallengeId} updated by {UpdatedBy} at {OccurredAt}. Changed fields: {ChangedFields}. Reason: {Reason}", 
                    domainEvent.ChallengeId, 
                    domainEvent.UpdatedByName,
                    domainEvent.OccurredAt,
                    changedFieldsText,
                    domainEvent.UpdateReason ?? "No reason provided");

                // Future: Could add additional side effects here such as:
                // - Send notifications to relevant stakeholders
                // - Update search indexes
                // - Trigger approval workflows if significant changes
                // - Update external systems
                // - Generate audit reports

                _logger.LogInformation("Successfully processed ChallengeUpdatedEvent for challenge {ChallengeId}. " +
                    "Updated by {UpdatedBy}, changed fields: {ChangedFields}", 
                    domainEvent.ChallengeId, domainEvent.UpdatedByName, changedFieldsText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ChallengeUpdatedEvent for challenge {ChallengeId}", 
                    domainEvent.ChallengeId);
                throw;
            }
            
            return Task.CompletedTask;
        }
    }
}
