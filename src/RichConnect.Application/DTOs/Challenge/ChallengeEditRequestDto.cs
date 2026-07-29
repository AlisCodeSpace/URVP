namespace RICHConnect.Backend.Application.DTOs.Challenge
{
    /// <summary>
    /// DTO representing a challenge edit request
    /// Returned when a Community Partner successfully requests an edit
    /// </summary>
    public class ChallengeEditRequestDto
    {
        /// <summary>
        /// Unique identifier for the edit request
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID of the challenge that needs editing
        /// </summary>
        public Guid ChallengeId { get; set; }

        /// <summary>
        /// The reason provided by the Community Partner for the edit request
        /// </summary>
        public string EditReason { get; set; } = null!;

        /// <summary>
        /// ID of the user who requested the edit
        /// </summary>
        public Guid RequestedBy { get; set; }

        /// <summary>
        /// Name of the user who requested the edit
        /// </summary>
        public string? RequestedByName { get; set; }

        /// <summary>
        /// When the edit request was created
        /// </summary>
        public DateTime RequestedAt { get; set; }

        /// <summary>
        /// Current status of the edit request (Pending, Approved, Rejected)
        /// </summary>
        public int Status { get; set; } = 0; // 0 = Pending

        /// <summary>
        /// Admin's response to the edit request (if any)
        /// </summary>
        public string? AdminResponse { get; set; }

        /// <summary>
        /// When the admin responded to the request (if any)
        /// </summary>
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// ID of the admin who responded (if any)
        /// </summary>
        public Guid? RespondedBy { get; set; }
    }
}
