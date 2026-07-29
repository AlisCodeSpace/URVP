using System.ComponentModel.DataAnnotations;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Challenges;
using RICHConnect.Backend.Domain.Entities.Themes;
using RICHConnect.Backend.Domain.Entities.Faculty;
using RICHConnect.Backend.Domain.Entities.Admin;
namespace RICHConnect.Backend.Domain.Entities.Users
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(128)]
        public string Name { get; set; } = null!;
        
        // Azure B2C integration
        [MaxLength(255)]
        public string? B2CUserId { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [Required]
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // ----------- Add this property -----------
        /// <summary>
        /// (Optional) URL to a profile image, if you plan to support that.
        /// </summary>
        [MaxLength(512)]
        public string? ProfileImageUrl { get; set; }

        // -------------------------------------------
        // Navigation Properties (unchanged)
        // -------------------------------------------

        public FacultySpecialist? FacultySpecialist { get; set; }


        public ICollection<ResearchTheme>? ThemesSubmitted { get; set; }
        public ICollection<Challenge>? ChallengesSubmitted { get; set; }
        public ICollection<AdminActionLog>? AdminActions { get; set; }
        public ICollection<Notifications.Notification>? Notifications { get; set; }
        public Notifications.UserNotificationSettings? NotificationSettings { get; set; }

    }
}
