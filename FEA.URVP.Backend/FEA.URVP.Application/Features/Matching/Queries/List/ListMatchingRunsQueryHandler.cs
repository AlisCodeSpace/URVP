using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.Matching.DTOs;
using FEA.URVP.Application.Features.Matching.Mappings;
using FEA.URVP.Domain.Entities.Matching;
using MediatR;

namespace FEA.URVP.Application.Features.Matching.Queries.List;

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
        if (!request.IncludeManual)
        {
            runs = runs
                .Where(r => r.AlgorithmVersion != MatchingRun.ManualAlgorithmVersion)
                .ToList();
        }

        return runs.Select(r => r.ToDto()).ToList();
    }
}
