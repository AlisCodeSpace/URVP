namespace FEA.URVP.Application.DTOs.News;

public sealed class NewsArticleDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Excerpt { get; init; } = null!;
    public string Category { get; init; } = null!;
    public string Author { get; init; } = null!;
    public string Ticker { get; init; } = null!;
    public IReadOnlyList<string> Body { get; init; } = [];
    public DateTime PublishedAt { get; init; }
    public bool Featured { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
