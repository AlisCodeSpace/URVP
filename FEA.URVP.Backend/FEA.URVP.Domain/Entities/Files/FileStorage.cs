using System.ComponentModel.DataAnnotations;

namespace FEA.URVP.Domain.Entities.Files;

/// <summary>File stored as a SQL varbinary blob (RichConnect FileStorage pattern).</summary>
public class FileStorage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string EntityType { get; set; } = null!;

    [Required]
    public Guid EntityId { get; set; }

    [Required, MaxLength(50)]
    public string FileCategory { get; set; } = null!;

    [Required, MaxLength(260)]
    public string FileName { get; set; } = null!;

    [Required, MaxLength(100)]
    public string MimeType { get; set; } = null!;

    [Required]
    public long FileSize { get; set; }

    [Required, MaxLength(32)]
    public byte[] ContentHash { get; set; } = [];

    [Required]
    public byte[] Content { get; set; } = [];

    public Guid? UploadedBy { get; set; }

    [Required]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsDeleted { get; set; }
}
