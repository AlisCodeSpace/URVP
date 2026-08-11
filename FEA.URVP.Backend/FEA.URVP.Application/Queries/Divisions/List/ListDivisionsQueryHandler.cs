using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Divisions;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Divisions.List;

public sealed class ListDivisionsQueryHandler
    : IRequestHandler<ListDivisionsQuery, (IReadOnlyList<DivisionDto> Items, int TotalCount)>
{
    private readonly IDivisionRepository _divisions;

    public ListDivisionsQueryHandler(IDivisionRepository divisions)
    {
        _divisions = divisions;
    }

    public async Task<(IReadOnlyList<DivisionDto> Items, int TotalCount)> Handle(
        ListDivisionsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _divisions.ListAsync(
            request.Search,
            request.ActiveOnly,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return (items.Select(x => x.ToDto()).ToList(), totalCount);
    }
}
