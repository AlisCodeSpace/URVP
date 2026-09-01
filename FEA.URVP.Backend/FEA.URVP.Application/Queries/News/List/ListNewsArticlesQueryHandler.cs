using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.News;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.News.List;

public sealed class ListNewsArticlesQueryHandler
    : IRequestHandler<ListNewsArticlesQuery, (IReadOnlyList<NewsArticleDto> Items, int TotalCount)>
{
    private readonly INewsArticleRepository _news;

    public ListNewsArticlesQueryHandler(INewsArticleRepository news)
    {
        _news = news;
    }

    public async Task<(IReadOnlyList<NewsArticleDto> Items, int TotalCount)> Handle(
        ListNewsArticlesQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _news.ListAsync(
            request.Search,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return (items.Select(x => x.ToDto()).ToList(), totalCount);
    }
}
