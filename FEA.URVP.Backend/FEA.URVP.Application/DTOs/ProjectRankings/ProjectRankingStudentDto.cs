namespace FEA.URVP.Application.DTOs.ProjectRankings;

public sealed class ProjectRankingStudentDto
{
    public Guid RankingId { get; init; }
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = null!;
    public string StudentEmail { get; init; } = null!;
    public string? StudentUserName { get; init; }
    public byte Rank { get; init; }
    public byte? FacultyRank { get; init; }
    public DateTime RankedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
