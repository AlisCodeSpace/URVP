using FEA.URVP.Application.Features.ProjectRankings.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.ProjectRankings.Queries.GetMine;

public sealed record GetMyProjectRankingsQuery(Guid CurrentUserId)
    : IRequest<IReadOnlyList<ProjectRankingDto>>;
