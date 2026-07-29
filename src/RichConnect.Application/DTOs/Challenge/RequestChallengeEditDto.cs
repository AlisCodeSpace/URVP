using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Application.DTOs.Challenge
{
    /// <summary>
    /// DTO for requesting a challenge edit
    /// Contains the reason why the Community Partner wants to edit their challenge
    /// </summary>
    public class RequestChallengeEditDto
    {
        /// <summary>
        /// Detailed reason explaining what needs to be edited and why
        /// </summary>
        [Required(ErrorMessage = "Edit reason is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Edit reason must be between 10 and 1000 characters")]
        public string EditReason { get; set; } = null!;
    }
}
