using System.ComponentModel.DataAnnotations;

namespace FEA.URVP.Domain.Entities.Notifications;

public class EmailLog
{
    public const int AddressMaxLength = 256;
    public const int RecipientsMaxLength = 1000;
    public const int ExceptionMaxLength = 4000;

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(AddressMaxLength)]
    public string From { get; set; } = null!;

    [Required, MaxLength(AddressMaxLength)]
    public string To { get; set; } = null!;

    [MaxLength(RecipientsMaxLength)]
    public string? Cc { get; set; }

    [MaxLength(RecipientsMaxLength)]
    public string? Bcc { get; set; }

    public string Body { get; set; } = null!;

    [MaxLength(ExceptionMaxLength)]
    public string? Exception { get; set; }

    [Required]
    public bool Success { get; set; }

    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;
}
