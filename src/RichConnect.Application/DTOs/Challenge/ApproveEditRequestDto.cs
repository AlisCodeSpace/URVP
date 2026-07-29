using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Application.DTOs.Challenge
{
    /// <summary>
    /// DTO for approving a challenge edit request
    /// Contains optional admin response/notes
    /// </summary>
    public class ApproveEditRequestDto
    {
        /// <summary>
        /// Optional admin response or notes about the approval
        /// </summary>
        [StringLength(1000, ErrorMessage = "Admin response must not exceed 1000 characters")]
        public string? AdminResponse { get; set; }
    }
}
