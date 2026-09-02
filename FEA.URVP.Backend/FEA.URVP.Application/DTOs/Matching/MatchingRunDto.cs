using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.DTOs.Matching;

public sealed class MatchingRunDto
{
    public Guid Id { get; init; }
    public Guid SemesterId { get; init; }
    public string SemesterName { get; init; } = null!;
    public MatchingRunStatus Status { get; init; }
    public string AlgorithmVersion { get; init; } = null!;
    public int Seed { get; init; }
    public int StudentsConsidered { get; init; }
    public int ProjectsConsidered { get; init; }
    public int SeatsAvailable { get; init; }
    public int StudentsMatched { get; init; }
    public int TieBreaksUsed { get; init; }
    public int WarningCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
}

public sealed class MatchingRunDetailDto
{
    public MatchingRunDto Run { get; init; } = null!;
    public IReadOnlyList<string> Warnings { get; init; } = [];
    public IReadOnlyList<PlacementDto> Placements { get; init; } = [];
}

public sealed class PlacementDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectTitle { get; init; } = null!;
    public string FacultyName { get; init; } = null!;
    public Guid StudentUserId { get; init; }
    public string StudentName { get; init; } = null!;
    public string StudentEmail { get; init; } = null!;
    public byte StudentRank { get; init; }
    public byte FacultyRank { get; init; }
    public bool ResolvedByTieBreak { get; init; }
    public PlacementStatus Status { get; init; }
    public DateTime UpdatedAt { get; init; }
}
