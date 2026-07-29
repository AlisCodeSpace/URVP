using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Entities.Faculty;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Domain.Entities.Challenges
{
    /// <summary>
    /// Represents a many-to-many relationship between Challenges and matched FacultySpecialists
    /// </summary>
    public class ChallengeMatchedFacultySpecialist
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
        public User FacultySpecialist { get; set; } = null!;

        [Required]
        public DateTime MatchedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public Guid MatchedByUserId { get; set; }

        [ForeignKey(nameof(MatchedByUserId))]
        public User MatchedByUser { get; set; } = null!;
    }
}
