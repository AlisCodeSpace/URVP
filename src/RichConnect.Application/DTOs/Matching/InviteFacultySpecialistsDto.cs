using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RICHConnect.Backend.Application.DTOs.Matching
{
    /// <summary>
    /// DTO for inviting professors to a challenge
    /// </summary>
    public class InviteFacultySpecialistsDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one facultySpecialist must be invited")]
        [JsonPropertyName("FacultySpecialistIds")]
        public List<Guid> FacultySpecialistIds { get; set; } = new();
    }
}
