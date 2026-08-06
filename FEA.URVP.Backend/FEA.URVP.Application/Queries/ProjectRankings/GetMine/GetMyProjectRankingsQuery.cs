using FEA.URVP.Application.DTOs.ProjectRankings;
using MediatR;

namespace FEA.URVP.Application.Queries.ProjectRankings.GetMine;

public sealed record GetMyProjectRankingsQuery(Guid CurrentUserId)
    : IRequest<IReadOnlyList<ProjectRankingDto>>;
