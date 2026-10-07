namespace FEA.URVP.Application.DTOs.Matching;

/// <summary>
/// A project the signed-in student is participating in. Rank is 0 when the
/// student was assigned without ranking the project.
/// </summary>
public sealed class MyPlacementDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectTitle { get; init; } = null!;
    public string FacultyName { get; init; } = null!;
    public string FacultyAffiliation { get; init; } = null!;
    public string SemesterName { get; init; } = "";
    public string BriefDescription { get; init; } = null!;
    public IReadOnlyList<string> ResearchAreas { get; init; } = [];
    public IReadOnlyList<string> ActivityTypes { get; init; } = [];
    public byte ProjectStatus { get; init; }
    public byte StudentRank { get; init; }
    public DateTime AssignedAt { get; init; }
}
