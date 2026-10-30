using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.DTOs.ProjectRankings;

namespace FEA.URVP.Application.DTOs.Users;

public sealed class StudentProjectActivityDto
{
    public IReadOnlyList<ProjectRankingDto> Rankings { get; init; } = [];

    public IReadOnlyList<PlacementDto> Assignments { get; init; } = [];
}
