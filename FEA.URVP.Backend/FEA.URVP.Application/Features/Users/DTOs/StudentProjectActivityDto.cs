using FEA.URVP.Application.Features.Matching.DTOs;
using FEA.URVP.Application.Features.ProjectRankings.DTOs;

namespace FEA.URVP.Application.Features.Users.DTOs;

public sealed class StudentProjectActivityDto
{
    public IReadOnlyList<ProjectRankingDto> Rankings { get; init; } = [];

    public IReadOnlyList<PlacementDto> Assignments { get; init; } = [];
}
