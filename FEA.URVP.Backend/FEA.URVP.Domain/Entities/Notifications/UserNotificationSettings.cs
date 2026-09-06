using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Domain.Entities.Notifications;

public class UserNotificationSettings
{
    [Key]
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    [Required]
    public bool EmailNotifications { get; set; } = true;

    [Required]
    public bool InAppNotifications { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
