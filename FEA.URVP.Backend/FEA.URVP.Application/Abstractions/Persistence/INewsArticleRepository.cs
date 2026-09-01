using FEA.URVP.Domain.Entities.News;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface INewsArticleRepository
{
    Task<NewsArticle?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<NewsArticle?> FindBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<NewsArticle> Items, int TotalCount)> ListAsync(
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NewsArticle>> ListAllAsync(CancellationToken cancellationToken = default);

    Task ClearFeaturedAsync(Guid? exceptId, CancellationToken cancellationToken = default);

    void Add(NewsArticle article);

    void Remove(NewsArticle article);
}
