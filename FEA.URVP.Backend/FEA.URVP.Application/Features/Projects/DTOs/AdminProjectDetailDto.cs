using FEA.URVP.Application.Features.Matching.DTOs;
using FEA.URVP.Application.Features.ProjectRankings.DTOs;

namespace FEA.URVP.Application.Features.Projects.DTOs;

public sealed class AdminProjectDetailDto
{
    public required ProjectDto Project { get; init; }
    public IReadOnlyList<PlacementDto> Assignments { get; init; } = [];
    public IReadOnlyList<ProjectRankingStudentDto> Rankings { get; init; } = [];
}
