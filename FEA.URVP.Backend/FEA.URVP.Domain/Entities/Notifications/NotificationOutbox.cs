using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Domain.Entities.Notifications;

public class NotificationOutbox
{
    public const int EventTypeMaxLength = 100;
    public const int StatusMaxLength = 32;
    public const int ErrorMessageMaxLength = 4000;

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid NotificationId { get; set; }

    public Notification Notification { get; set; } = null!;

    [Required, MaxLength(EventTypeMaxLength)]
    public string EventType { get; set; } = NotificationOutboxEventTypes.EmailNotification;

    [Required, MaxLength(StatusMaxLength)]
    public string Status { get; set; } = nameof(NotificationOutboxStatus.Pending);

    [Required]
    public int RetryCount { get; set; }

    public DateTime? NextRetryAt { get; set; }

    [MaxLength(ErrorMessageMaxLength)]
    public string? ErrorMessage { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }
}
