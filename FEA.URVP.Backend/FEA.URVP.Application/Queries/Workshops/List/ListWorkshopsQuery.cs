using FEA.URVP.Application.DTOs.Workshops;
using MediatR;

namespace FEA.URVP.Application.Queries.Workshops.List;

public sealed class ListWorkshopsQuery
    : IRequest<(IReadOnlyList<WorkshopDto> Items, int TotalCount)>
{
    public string? Search { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public ListWorkshopsQuery(string? search, int pageNumber, int pageSize)
    {
        Search = search;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
