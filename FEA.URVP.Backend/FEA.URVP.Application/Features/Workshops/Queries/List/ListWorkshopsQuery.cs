using FEA.URVP.Application.DTOs.Workshops;
using MediatR;

namespace FEA.URVP.Application.Queries.Workshops.List;

public sealed class ListWorkshopsQuery
    : IRequest<(IReadOnlyList<WorkshopDto> Items, int TotalCount)>
{
    public string? Search { get; }
    public bool PublishedOnly { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public ListWorkshopsQuery(string? search, bool publishedOnly, int pageNumber, int pageSize)
    {
        Search = search;
        PublishedOnly = publishedOnly;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
