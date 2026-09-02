using FEA.URVP.Application.DTOs.Matching;
using MediatR;

namespace FEA.URVP.Application.Queries.Matching.List;

public sealed record ListMatchingRunsQuery(Guid? SemesterId) : IRequest<IReadOnlyList<MatchingRunDto>>;
