using FEA.URVP.Application.DTOs.News;
using MediatR;

namespace FEA.URVP.Application.Queries.News.List;

public sealed class ListNewsArticlesQuery
    : IRequest<(IReadOnlyList<NewsArticleDto> Items, int TotalCount)>
{
    public string? Search { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public ListNewsArticlesQuery(string? search, int pageNumber, int pageSize)
    {
        Search = search;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
