namespace RICHConnect.Backend.Domain.Events
{
    /// <summary>
    /// Domain event raised when an admin rejects a challenge edit request
    /// This event can be used to notify the Community Partner and trigger other business processes
    /// </summary>
    public class ChallengeEditRequestRejectedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ChallengeEditRequestRejected";

        /// <summary>
        /// ID of the challenge that was rejected for editing
        /// </summary>
        public Guid ChallengeId { get; }

        /// <summary>
        /// ID of the edit request
        /// </summary>
        public Guid EditRequestId { get; }

        /// <summary>
        /// ID of the Community Partner who requested the edit
        /// </summary>
        public Guid RequestedBy { get; }

        /// <summary>
        /// ID of the admin who rejected the request
        /// </summary>
        public Guid RejectedBy { get; }

        /// <summary>
        /// When the request was rejected
        /// </summary>
        public DateTime RejectedAt { get; }

        /// <summary>
        /// Admin response explaining the rejection
        /// </summary>
        public string AdminResponse { get; }

        /// <summary>
        /// Optional correlation ID for tracking
        /// </summary>
        public string? CorrelationId { get; }

        public ChallengeEditRequestRejectedEvent(
            Guid editRequestId,
            Guid challengeId,
            Guid requestedBy,
            Guid rejectedBy,
            DateTime rejectedAt,
            string adminResponse,
            string? correlationId = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            EditRequestId = editRequestId;
            ChallengeId = challengeId;
            RequestedBy = requestedBy;
            RejectedBy = rejectedBy;
            RejectedAt = rejectedAt;
            AdminResponse = adminResponse;
            CorrelationId = correlationId;
        }
    }
}
