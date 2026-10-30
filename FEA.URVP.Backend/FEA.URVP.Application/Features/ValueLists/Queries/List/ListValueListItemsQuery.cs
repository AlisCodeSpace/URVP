using FEA.URVP.Application.Features.ValueLists.DTOs;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Features.ValueLists.Queries.List;

public sealed class ListValueListItemsQuery
    : IRequest<(IReadOnlyList<ValueListItemDto> Items, int TotalCount)>
{
    public ValueListKind Kind { get; }
    public string? Search { get; }
    public bool ActiveOnly { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public ListValueListItemsQuery(
        ValueListKind kind,
        string? search,
        bool activeOnly,
        int pageNumber,
        int pageSize)
    {
        Kind = kind;
        Search = search;
        ActiveOnly = activeOnly;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
