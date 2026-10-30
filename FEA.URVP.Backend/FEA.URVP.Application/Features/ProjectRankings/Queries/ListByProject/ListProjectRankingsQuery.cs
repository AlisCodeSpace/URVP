using FEA.URVP.Application.Features.ProjectRankings.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.ProjectRankings.Queries.ListByProject;

public sealed record ListProjectRankingsQuery(
    Guid ProjectId,
    Guid CurrentUserId,
    bool IsAdmin) : IRequest<IReadOnlyList<ProjectRankingStudentDto>>;
