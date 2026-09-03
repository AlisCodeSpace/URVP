namespace FEA.URVP.Application.DTOs.ProjectRankings;

public sealed class ProjectRankingDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public byte Rank { get; init; }
    public string ProjectTitle { get; init; } = null!;
    public string FacultyName { get; init; } = null!;
    public string FacultyAffiliation { get; init; } = null!;
    public IReadOnlyList<string> ResearchAreas { get; init; } = [];
    public byte ProjectStatus { get; init; }
    public bool IsMatched { get; init; }
    public DateTime RankedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
