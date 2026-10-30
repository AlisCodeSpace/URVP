using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Semesters.List;

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
