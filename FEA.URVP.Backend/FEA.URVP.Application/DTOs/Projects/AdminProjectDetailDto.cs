using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.DTOs.ProjectRankings;

namespace FEA.URVP.Application.DTOs.Projects;

public sealed class AdminProjectDetailDto
{
    public required ProjectDto Project { get; init; }
    public IReadOnlyList<PlacementDto> Assignments { get; init; } = [];
    public IReadOnlyList<ProjectRankingStudentDto> Rankings { get; init; } = [];
}
