using System.ComponentModel.DataAnnotations;

namespace FEA.URVP.Domain.Entities.Email;

/// <summary>
/// Singleton row for SMTP credentials. The password is stored only as a
/// data-protection payload — never plaintext, never returned to clients.
/// </summary>
public class EmailSettings
{
    public const int UserNameMaxLength = 256;
    public const int PasswordPlaintextMaxLength = 256;
    public const int ProtectedPasswordMaxLength = 4000;

    /// <summary>Only one EmailSettings row exists.</summary>
    public static readonly Guid SingletonId = Guid.Parse("a3c1e5d0-8b24-4f6a-9e17-2d4c8f91b0e3");

    [Key]
    public Guid Id { get; set; } = SingletonId;

    [MaxLength(UserNameMaxLength)]
    public string? UserName { get; set; }

    /// <summary>Data-protection ciphertext. Do not log or expose.</summary>
    [MaxLength(ProtectedPasswordMaxLength)]
    public string? ProtectedPassword { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? UpdatedByUserId { get; set; }

    public bool HasPassword => !string.IsNullOrEmpty(ProtectedPassword);
}
