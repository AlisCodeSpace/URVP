using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.News;
using MediatR;

namespace FEA.URVP.Application.Commands.News.Update;

public sealed class UpdateNewsArticleCommand : IRequest<NewsArticleDto>
{
    [JsonIgnore]
    public Guid Id { get; set; }

    public string? Slug { get; init; }
    public string Title { get; init; } = null!;
    public string Excerpt { get; init; } = null!;
    public string Category { get; init; } = null!;
    public string Author { get; init; } = null!;
    public string Ticker { get; init; } = null!;
    public List<string> Body { get; init; } = [];
    public DateTime PublishedAt { get; init; }
    public bool Featured { get; init; }
}
