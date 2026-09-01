using System.ComponentModel.DataAnnotations;

namespace FEA.URVP.Domain.Entities.Workshops;

public class Workshop
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(256)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(64)]
    public string Date { get; set; } = null!;

    [MaxLength(64)]
    public string? Time { get; set; }

    [MaxLength(256)]
    public string? Location { get; set; }

    [Required, MaxLength(2000)]
    public string Description { get; set; } = null!;

    [Required, MaxLength(500)]
    public string RegistrationUrl { get; set; } = null!;

    public Guid? PosterFileId { get; set; }

    [MaxLength(256)]
    public string? PosterAlt { get; set; }

    [Required]
    public int SortOrder { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
