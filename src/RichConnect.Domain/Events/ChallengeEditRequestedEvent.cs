namespace RICHConnect.Backend.Domain.Events
{
    /// <summary>
    /// Domain event raised when a Community Partner requests an edit for their challenge
    /// This event can be used to notify admins and trigger other business processes
    /// </summary>
    public class ChallengeEditRequestedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ChallengeEditRequested";

        /// <summary>
        /// ID of the challenge that needs editing
        /// </summary>
        public Guid ChallengeId { get; }

        /// <summary>
        /// ID of the edit request
        /// </summary>
        public Guid EditRequestId { get; }

        /// <summary>
        /// Title of the challenge
        /// </summary>
        public string ChallengeTitle { get; }

        /// <summary>
        /// ID of the Community Partner who requested the edit
        /// </summary>
        public Guid RequestedByUserId { get; }

        /// <summary>
        /// Name of the Community Partner who requested the edit
        /// </summary>
        public string RequestedByName { get; }

        /// <summary>
        /// Email of the Community Partner who requested the edit
        /// </summary>
        public string RequestedByEmail { get; }

        /// <summary>
        /// The reason provided for the edit request
        /// </summary>
        public string EditReason { get; }

        /// <summary>
        /// Current status of the challenge
        /// </summary>
        public string ChallengeStatus { get; }

        /// <summary>
        /// Optional correlation ID for tracking
        /// </summary>
        public string? CorrelationId { get; }

        public ChallengeEditRequestedEvent(
            Guid challengeId,
            Guid editRequestId,
            string challengeTitle,
            Guid requestedByUserId,
            string requestedByName,
            string requestedByEmail,
            string editReason,
            string challengeStatus,
            string? correlationId = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            ChallengeId = challengeId;
            EditRequestId = editRequestId;
            ChallengeTitle = challengeTitle;
            RequestedByUserId = requestedByUserId;
            RequestedByName = requestedByName;
            RequestedByEmail = requestedByEmail;
            EditReason = editReason;
            ChallengeStatus = challengeStatus;
            CorrelationId = correlationId;
        }
    }
}
