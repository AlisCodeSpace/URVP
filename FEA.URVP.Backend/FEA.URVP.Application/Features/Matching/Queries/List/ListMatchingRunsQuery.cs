using FEA.URVP.Application.Features.Matching.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Matching.Queries.List;

public sealed record ListMatchingRunsQuery(Guid? SemesterId, bool IncludeManual = false)
    : IRequest<IReadOnlyList<MatchingRunDto>>;
