using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Workshops;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Workshops.List;

public sealed class ListWorkshopsQueryHandler
    : IRequestHandler<ListWorkshopsQuery, (IReadOnlyList<WorkshopDto> Items, int TotalCount)>
{
    private readonly IWorkshopRepository _workshops;

    public ListWorkshopsQueryHandler(IWorkshopRepository workshops)
    {
        _workshops = workshops;
    }

    public async Task<(IReadOnlyList<WorkshopDto> Items, int TotalCount)> Handle(
        ListWorkshopsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _workshops.ListAsync(
            request.Search,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return (items.Select(x => x.ToDto()).ToList(), totalCount);
    }
}
