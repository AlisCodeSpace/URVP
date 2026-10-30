using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.ValueLists.DTOs;
using FEA.URVP.Application.Features.ValueLists.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.ValueLists.Queries.List;

public sealed class ListValueListItemsQueryHandler
    : IRequestHandler<ListValueListItemsQuery, (IReadOnlyList<ValueListItemDto> Items, int TotalCount)>
{
    private readonly IValueListRepository _valueLists;

    public ListValueListItemsQueryHandler(IValueListRepository valueLists)
    {
        _valueLists = valueLists;
    }

    public async Task<(IReadOnlyList<ValueListItemDto> Items, int TotalCount)> Handle(
        ListValueListItemsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _valueLists.ListByKindAsync(
            request.Kind,
            request.Search,
            request.ActiveOnly,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return (items.Select(x => x.ToDto()).ToList(), totalCount);
    }
}
