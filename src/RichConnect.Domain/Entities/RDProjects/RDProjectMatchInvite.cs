using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Faculty;

namespace RICHConnect.Backend.Domain.Entities.RDProjects
{
    public class RDProjectMatchInvite
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
        public Users.User FacultySpecialist { get; set; } = null!;

        [Required]
        public RDProjectInviteStatus Status { get; set; } = RDProjectInviteStatus.Pending;

        [Required]
        public DateTime InvitedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
