using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Application.DTOs.Faculty
{
    public class UpdateFacultySpecialistStatusDto
    {
        [Required]
        public int Status { get; set; } // 0 = Unavailable, 1 = Available
    }
} 