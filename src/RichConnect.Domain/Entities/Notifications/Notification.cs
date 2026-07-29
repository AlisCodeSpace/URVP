using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RICHConnect.Backend.Domain.Entities.Users;

namespace RICHConnect.Backend.Domain.Entities.Notifications
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required, MaxLength(1000)]
        public string Message { get; set; } = null!;

        [MaxLength(4000)]
        public string? Data { get; set; } // JSON data for additional context

        public Guid? ReferenceId { get; set; } // Links to Challenge/Partner/Theme/ResearchField
        
        [MaxLength(50)]
        public string? ReferenceType { get; set; } // "Challenge", "Partner", "Theme", "ResearchField"

        [Required]
        public bool IsRead { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        [MaxLength(20)]
        public string Priority { get; set; } = "low";

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
