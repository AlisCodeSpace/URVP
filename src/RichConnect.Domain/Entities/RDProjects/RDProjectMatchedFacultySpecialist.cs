using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Entities.Faculty;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Domain.Entities.RDProjects
{
    public class RDProjectMatchedFacultySpecialist
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RDProjectId { get; set; }

        [ForeignKey(nameof(RDProjectId))]
        public RDProject RDProject { get; set; } = null!;

        [Required]
        public Guid FacultySpecialistUserId { get; set; }

        [ForeignKey(nameof(FacultySpecialistUserId))]
        public User FacultySpecialist { get; set; } = null!;

        [Required]
        public Guid MatchedByUserId { get; set; }

        [ForeignKey(nameof(MatchedByUserId))]
        public User MatchedByUser { get; set; } = null!;

        [Required]
        public DateTime MatchedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
