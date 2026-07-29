using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Application.DTOs.Challenge
{
    /// <summary>
    /// DTO for rejecting a Challenge
    /// </summary>
    public class RejectChallengeDto
    {
        /// <summary>
        /// Required reason for rejection
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string RejectionReason { get; set; } = null!;
    }
}