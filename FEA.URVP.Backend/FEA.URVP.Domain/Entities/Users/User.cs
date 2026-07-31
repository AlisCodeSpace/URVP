using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Domain.Entities.Users;

public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(64)]
    public string UserName { get; set; } = null!;

    [Required, MaxLength(256)]
    public string Affiliation { get; set; } = null!;

    [Required]
    public UserRole Role { get; set; }

    [MaxLength(512)]
    public string? ProfileImageUrl { get; set; }

    [Required]
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
