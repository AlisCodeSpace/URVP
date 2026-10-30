namespace FEA.URVP.Application.DTOs.Matching;

public sealed class AssignmentCandidateDto
{
    public Guid UserId { get; init; }

    public string Name { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? Faculty { get; init; }

    public string? Major { get; init; }

    public string? Degree { get; init; }

    public bool HasProfile { get; init; }

    public IReadOnlyList<string> MatchedResearchTopics { get; init; } = [];

    public bool QualificationsMentionMajor { get; init; }

    public bool QualificationsMentionFaculty { get; init; }

    public Guid? AssignedProjectId { get; init; }

    public string? AssignedProjectTitle { get; init; }
}
