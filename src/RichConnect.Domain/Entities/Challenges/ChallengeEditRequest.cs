using System.ComponentModel.DataAnnotations;

using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Users;
namespace RICHConnect.Backend.Domain.Entities.Challenges
{
    /// <summary>
    /// Entity representing a request to edit a challenge
    /// Community Partners can request edits for their submitted challenges
    /// </summary>
    public class ChallengeEditRequest
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
        [Required]
        [MaxLength(1000)]
        public string EditReason { get; set; } = null!;

        /// <summary>
        /// ID of the user who requested the edit
        /// </summary>
        public Guid RequestedBy { get; set; }

        /// <summary>
        /// When the edit request was created
        /// </summary>
        public DateTime RequestedAt { get; set; }

        /// <summary>
        /// Current status of the edit request (Pending, Approved, Rejected)
        /// </summary>
        [Required]
        public EditRequestStatus Status { get; set; } = EditRequestStatus.Pending;

        /// <summary>
        /// Admin's response to the edit request (if any)
        /// </summary>
        [MaxLength(1000)]
        public string? AdminResponse { get; set; }

        /// <summary>
        /// When the admin responded to the request (if any)
        /// </summary>
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// ID of the admin who responded (if any)
        /// </summary>
        public Guid? RespondedBy { get; set; }

        /// <summary>
        /// When the record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Row version for concurrency control
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation properties
        /// <summary>
        /// The challenge that this edit request is for
        /// </summary>
        public Challenge Challenge { get; set; } = null!;

        /// <summary>
        /// The user who requested the edit
        /// </summary>
        public User RequestedByUser { get; set; } = null!;

        /// <summary>
        /// The admin who responded to the request (if any)
        /// </summary>
        public User? RespondedByUser { get; set; }
    }
}
