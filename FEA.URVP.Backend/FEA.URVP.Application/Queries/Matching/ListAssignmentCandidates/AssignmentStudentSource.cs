namespace FEA.URVP.Application.Queries.Matching.ListAssignmentCandidates;

/// <summary>Slim student row used to score and page assignment candidates.</summary>
public sealed record AssignmentStudentSource(
    Guid UserId,
    string Name,
    string Email,
    string UserName,
    bool HasProfile,
    string? Faculty,
    string? Major,
    string? Degree,
    IReadOnlyList<string> ResearchTopics);

public sealed record ConfirmedPlacementLabel(Guid ProjectId, string Title);
