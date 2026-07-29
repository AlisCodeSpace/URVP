using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Application.DTOs.Challenge
{
    /// <summary>
    /// DTO for rejecting a challenge edit request
    /// Contains required admin response explaining the rejection
    /// </summary>
    public class RejectEditRequestDto
    {
        /// <summary>
        /// Required admin response explaining why the edit request was rejected
        /// </summary>
        [Required(ErrorMessage = "Admin response is required when rejecting an edit request")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Admin response must be between 10 and 1000 characters")]
        public string AdminResponse { get; set; } = null!;
    }
}
