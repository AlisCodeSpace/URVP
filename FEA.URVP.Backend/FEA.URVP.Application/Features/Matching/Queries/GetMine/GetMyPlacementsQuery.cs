using FEA.URVP.Application.Features.Matching.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Matching.Queries.GetMine;

public sealed record GetMyPlacementsQuery(Guid CurrentUserId)
    : IRequest<IReadOnlyList<MyPlacementDto>>;
