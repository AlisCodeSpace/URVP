using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Domain.Entities.Notifications
{
    public class NotificationTemplate
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string Type { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Subject { get; set; } = null!;

        [Required]
        public string Body { get; set; } = null!;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
