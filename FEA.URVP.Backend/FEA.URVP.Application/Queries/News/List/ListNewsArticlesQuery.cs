using FEA.URVP.Application.DTOs.News;
using MediatR;

namespace FEA.URVP.Application.Queries.News.List;

public sealed class ListNewsArticlesQuery
    : IRequest<(IReadOnlyList<NewsArticleDto> Items, int TotalCount)>
{
    public string? Search { get; }
    public bool PublishedOnly { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public ListNewsArticlesQuery(string? search, bool publishedOnly, int pageNumber, int pageSize)
    {
        Search = search;
        PublishedOnly = publishedOnly;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
