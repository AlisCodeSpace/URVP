using FEA.URVP.Application.DTOs.Matching;
using MediatR;

namespace FEA.URVP.Application.Queries.Matching.GetMine;

public sealed record GetMyPlacementsQuery(Guid CurrentUserId)
    : IRequest<IReadOnlyList<MyPlacementDto>>;
