using FEA.URVP.Application.Features.Matching.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Matching.Queries.GetById;

public sealed record GetMatchingRunQuery(Guid RunId) : IRequest<MatchingRunDetailDto>;
