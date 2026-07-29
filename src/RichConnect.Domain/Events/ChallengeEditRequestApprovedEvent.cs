namespace RICHConnect.Backend.Domain.Events
{
    /// <summary>
    /// Domain event raised when an admin approves a challenge edit request
    /// This event can be used to notify the Community Partner and trigger other business processes
    /// </summary>
    public class ChallengeEditRequestApprovedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "ChallengeEditRequestApproved";

        /// <summary>
        /// ID of the challenge that was approved for editing
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
        /// ID of the admin who approved the request
        /// </summary>
        public Guid ApprovedBy { get; }

        /// <summary>
        /// When the request was approved
        /// </summary>
        public DateTime ApprovedAt { get; }

        /// <summary>
        /// Optional admin response/notes about the approval
        /// </summary>
        public string? AdminResponse { get; }

        /// <summary>
        /// Optional correlation ID for tracking
        /// </summary>
        public string? CorrelationId { get; }

        public ChallengeEditRequestApprovedEvent(
            Guid editRequestId,
            Guid challengeId,
            Guid requestedBy,
            Guid approvedBy,
            DateTime approvedAt,
            string? adminResponse,
            string? correlationId = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            EditRequestId = editRequestId;
            ChallengeId = challengeId;
            RequestedBy = requestedBy;
            ApprovedBy = approvedBy;
            ApprovedAt = approvedAt;
            AdminResponse = adminResponse;
            CorrelationId = correlationId;
        }
    }
}
