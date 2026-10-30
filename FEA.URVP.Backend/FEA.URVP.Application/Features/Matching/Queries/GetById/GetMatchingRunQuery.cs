using FEA.URVP.Application.DTOs.Matching;
using MediatR;

namespace FEA.URVP.Application.Queries.Matching.GetById;

public sealed record GetMatchingRunQuery(Guid RunId) : IRequest<MatchingRunDetailDto>;
