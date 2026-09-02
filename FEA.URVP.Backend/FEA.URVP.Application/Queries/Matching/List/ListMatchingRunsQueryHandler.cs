using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Matching.List;

public sealed class ListMatchingRunsQueryHandler
    : IRequestHandler<ListMatchingRunsQuery, IReadOnlyList<MatchingRunDto>>
{
    private readonly IMatchingRunRepository _runs;

    public ListMatchingRunsQueryHandler(IMatchingRunRepository runs)
    {
        _runs = runs;
    }

    public async Task<IReadOnlyList<MatchingRunDto>> Handle(
        ListMatchingRunsQuery request,
        CancellationToken cancellationToken)
    {
        var runs = await _runs.ListAsync(request.SemesterId, cancellationToken);
        return runs.Select(r => r.ToDto()).ToList();
    }
}
