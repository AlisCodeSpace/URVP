using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Faculty;
namespace RICHConnect.Backend.Domain.Entities.Challenges
{
    public class ChallengeMatchInvite
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ChallengeId { get; set; }

        [ForeignKey(nameof(ChallengeId))]
        public Challenge Challenge { get; set; } = null!;

        [Required]
        public Guid FacultySpecialistUserId { get; set; }

        [ForeignKey(nameof(FacultySpecialistUserId))]
        public Users.User FacultySpecialist { get; set; } = null!;

        [Required]
        public InviteStatus Status { get; set; } = InviteStatus.Pending;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}


