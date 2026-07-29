using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RICHConnect.Backend.Domain.Entities.Notifications
{
    public class NotificationOutbox
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public Guid NotificationId { get; set; }
        
        [Required, MaxLength(50)]
        public string EventType { get; set; } = null!; // "EmailNotification", "PushNotification"
        
        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending", "Processing", "Completed", "Failed"
        
        public int RetryCount { get; set; } = 0;
        
        public DateTime? NextRetryAt { get; set; }
        
        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? ProcessedAt { get; set; }
        
        // Navigation property
        [ForeignKey(nameof(NotificationId))]
        public Notification Notification { get; set; } = null!;
    }
}
