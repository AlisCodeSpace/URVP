using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Application.DTOs.Themes
{
    public class RejectThemeDto
    {
        [Required]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Rejection reason must be between 10 and 1000 characters.")]
        public string RejectionReason { get; set; } = null!;
    }
}
