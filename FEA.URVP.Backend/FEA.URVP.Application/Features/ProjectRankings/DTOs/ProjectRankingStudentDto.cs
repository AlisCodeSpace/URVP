namespace FEA.URVP.Application.Features.ProjectRankings.DTOs;

public sealed class ProjectRankingStudentDto
{
    public Guid RankingId { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = null!;
    public string StudentEmail { get; init; } = null!;
    public string? StudentUserName { get; init; }
    public byte Rank { get; init; }
    public byte? FacultyRank { get; init; }
    public Guid? RankedByUserId { get; init; }
    public string? RankedByName { get; init; }
    public Guid? AssignedPlacementId { get; init; }
    public Guid? AssignedProjectId { get; init; }
    public string? AssignedProjectTitle { get; init; }
    public DateTime RankedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
