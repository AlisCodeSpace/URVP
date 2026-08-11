using FEA.URVP.Application.DTOs.Divisions;
using MediatR;

namespace FEA.URVP.Application.Queries.Divisions.List;

public sealed class ListDivisionsQuery
    : IRequest<(IReadOnlyList<DivisionDto> Items, int TotalCount)>
{
    public string? Search { get; }
    public bool ActiveOnly { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public ListDivisionsQuery(
        string? search,
        bool activeOnly,
        int pageNumber,
        int pageSize)
    {
        Search = search;
        ActiveOnly = activeOnly;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
