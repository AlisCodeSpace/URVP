using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Entities.Faculty;

namespace RICHConnect.Backend.Domain.Entities.ResearchFields
{
    /// <summary>
    /// Junction table linking FacultySpecialists to ResearchFields
    /// </summary>
    public class FacultySpecialistResearchField
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid FacultySpecialistUserId { get; set; }
        
        [ForeignKey(nameof(FacultySpecialistUserId))]
        public FacultySpecialist FacultySpecialist { get; set; } = null!;

        [Required]
        public Guid ResearchFieldId { get; set; }
        
        [ForeignKey(nameof(ResearchFieldId))]
        public ResearchField ResearchField { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
