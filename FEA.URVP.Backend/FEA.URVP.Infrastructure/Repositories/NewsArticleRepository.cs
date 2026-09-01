using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.News;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class NewsArticleRepository : INewsArticleRepository
{
    private readonly AppDbContext _db;

    public NewsArticleRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<NewsArticle?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.NewsArticles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<NewsArticle?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _db.NewsArticles.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public async Task<(IReadOnlyList<NewsArticle> Items, int TotalCount)> ListAsync(
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.NewsArticles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                x.Title.Contains(term)
                || x.Excerpt.Contains(term)
                || x.Category.Contains(term)
                || x.Author.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Featured)
            .ThenByDescending(x => x.PublishedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<NewsArticle>> ListAllAsync(CancellationToken cancellationToken = default) =>
        await _db.NewsArticles.AsNoTracking()
            .OrderByDescending(x => x.Featured)
            .ThenByDescending(x => x.PublishedAt)
            .ToListAsync(cancellationToken);

    public async Task ClearFeaturedAsync(Guid? exceptId, CancellationToken cancellationToken = default)
    {
        var featured = await _db.NewsArticles
            .Where(x => x.Featured && (exceptId == null || x.Id != exceptId))
            .ToListAsync(cancellationToken);

        foreach (var article in featured)
        {
            article.Featured = false;
            article.UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Add(NewsArticle article) => _db.NewsArticles.Add(article);

    public void Remove(NewsArticle article) => _db.NewsArticles.Remove(article);
}
