using System.ComponentModel.DataAnnotations;

namespace FEA.URVP.Domain.Entities.News;

public class NewsArticle
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(160)]
    public string Slug { get; set; } = null!;

    [Required, MaxLength(256)]
    public string Title { get; set; } = null!;

    [Required, MaxLength(1000)]
    public string Excerpt { get; set; } = null!;

    [Required, MaxLength(64)]
    public string Category { get; set; } = null!;

    [Required, MaxLength(128)]
    public string Author { get; set; } = null!;

    [Required, MaxLength(256)]
    public string Ticker { get; set; } = null!;

    public List<string> Body { get; set; } = [];

    [Required]
    public DateTime PublishedAt { get; set; }

    [Required]
    public bool Featured { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
