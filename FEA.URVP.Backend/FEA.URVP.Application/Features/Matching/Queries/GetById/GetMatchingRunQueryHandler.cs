using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Matching.GetById;

public sealed class GetMatchingRunQueryHandler
    : IRequestHandler<GetMatchingRunQuery, MatchingRunDetailDto>
{
    private readonly IMatchingRunRepository _runs;

    public GetMatchingRunQueryHandler(IMatchingRunRepository runs)
    {
        _runs = runs;
    }

    public async Task<MatchingRunDetailDto> Handle(
        GetMatchingRunQuery request,
        CancellationToken cancellationToken)
    {
        var run = await _runs.GetDetailAsync(request.RunId, cancellationToken)
            ?? throw new KeyNotFoundException($"Matching run {request.RunId} was not found.");

        return run.ToDetailDto();
    }
}
