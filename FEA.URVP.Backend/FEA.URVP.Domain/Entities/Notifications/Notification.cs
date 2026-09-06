using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Domain.Entities.Notifications;

public class Notification
{
    public const int TypeMaxLength = 50;
    public const int TitleMaxLength = 200;
    public const int MessageMaxLength = 1000;
    public const int DataMaxLength = 4000;
    public const int ReferenceTypeMaxLength = 50;
    public const int PriorityMaxLength = 20;
    public const string DefaultPriority = "low";

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    [Required, MaxLength(TypeMaxLength)]
    public string Type { get; set; } = null!;

    [Required, MaxLength(TitleMaxLength)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(MessageMaxLength)]
    public string Message { get; set; } = null!;

    [MaxLength(DataMaxLength)]
    public string? Data { get; set; }

    public Guid? ReferenceId { get; set; }

    [MaxLength(ReferenceTypeMaxLength)]
    public string? ReferenceType { get; set; }

    [Required]
    public bool IsRead { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    [Required, MaxLength(PriorityMaxLength)]
    public string Priority { get; set; } = DefaultPriority;
}
