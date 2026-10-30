using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.Matching.DTOs;
using FEA.URVP.Application.Features.Matching.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.Matching.Queries.GetById;

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
