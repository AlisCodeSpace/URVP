using FEA.URVP.Application.DTOs.ProjectRankings;
using MediatR;

namespace FEA.URVP.Application.Queries.ProjectRankings.ListByProject;

public sealed record ListProjectRankingsQuery(
    Guid ProjectId,
    Guid CurrentUserId,
    bool IsAdmin) : IRequest<IReadOnlyList<ProjectRankingStudentDto>>;
