using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.News.DTOs;
using FEA.URVP.Application.Features.News.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.News.Queries.List;

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
            request.PublishedOnly,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return (items.Select(x => x.ToDto()).ToList(), totalCount);
    }
}
