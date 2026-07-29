using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Domain.Entities.Notifications
{
    public class UserNotificationSettings
    {
        [Key]
        public Guid UserId { get; set; }

        [Required]
        public bool EmailNotifications { get; set; } = true;

        [Required]
        public bool InAppNotifications { get; set; } = true;

        // Additional properties for compatibility (not mapped to database)
        [NotMapped]
        public bool EmailNotificationsEnabled 
        { 
            get => EmailNotifications; 
            set => EmailNotifications = value; 
        }

        [NotMapped]
        public bool PushNotificationsEnabled 
        { 
            get => InAppNotifications; 
            set => InAppNotifications = value; 
        }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
