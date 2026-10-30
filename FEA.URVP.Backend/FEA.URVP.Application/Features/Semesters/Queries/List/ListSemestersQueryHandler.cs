using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.Semesters.DTOs;
using FEA.URVP.Application.Features.Semesters.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.Semesters.Queries.List;

public sealed class ListSemestersQueryHandler
    : IRequestHandler<ListSemestersQuery, IReadOnlyList<SemesterDto>>
{
    private readonly ISemesterRepository _semesters;

    public ListSemestersQueryHandler(ISemesterRepository semesters)
    {
        _semesters = semesters;
    }

    public async Task<IReadOnlyList<SemesterDto>> Handle(
        ListSemestersQuery request,
        CancellationToken cancellationToken)
    {
        var items = await _semesters.ListAllAsync(cancellationToken);
        return items.Select(x => x.ToDto()).ToList();
    }
}
