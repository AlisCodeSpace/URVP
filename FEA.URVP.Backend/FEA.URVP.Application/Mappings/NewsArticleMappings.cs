using FEA.URVP.Application.DTOs.News;
using FEA.URVP.Domain.Entities.News;

namespace FEA.URVP.Application.Mappings;

public static class NewsArticleMappings
{
    public static NewsArticleDto ToDto(this NewsArticle article) => new()
    {
        Id = article.Id,
        Slug = article.Slug,
        Title = article.Title,
        Excerpt = article.Excerpt,
        Category = article.Category,
        Author = article.Author,
        Ticker = article.Ticker,
        Body = article.Body.ToList(),
        ImageFileIds = article.ImageFileIds.ToList(),
        PublishedAt = article.PublishedAt,
        Featured = article.Featured,
        Published = article.Published,
        CreatedAt = article.CreatedAt,
        UpdatedAt = article.UpdatedAt,
    };
}
