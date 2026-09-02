using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Domain.Entities.Matching;

namespace FEA.URVP.Application.Mappings;

public static class MatchingMappings
{
    public static MatchingRunDto ToDto(this MatchingRun run) => new()
    {
        Id = run.Id,
        SemesterId = run.SemesterId,
        SemesterName = run.Semester.Name,
        Status = run.Status,
        AlgorithmVersion = run.AlgorithmVersion,
        Seed = run.Seed,
        StudentsConsidered = run.StudentsConsidered,
        ProjectsConsidered = run.ProjectsConsidered,
        SeatsAvailable = run.SeatsAvailable,
        StudentsMatched = run.StudentsMatched,
        TieBreaksUsed = run.TieBreaksUsed,
        WarningCount = run.Warnings.Count,
        CreatedAt = run.CreatedAt,
        ConfirmedAt = run.ConfirmedAt,
    };

    public static MatchingRunDetailDto ToDetailDto(this MatchingRun run) => new()
    {
        Run = run.ToDto(),
        Warnings = run.Warnings,
        Placements = run.Placements
            .OrderBy(p => p.Project.Title)
            .ThenBy(p => p.FacultyRank)
            .ThenBy(p => p.StudentUser.Name)
            .Select(p => p.ToDto())
            .ToList(),
    };

    public static PlacementDto ToDto(this Placement placement) => new()
    {
        Id = placement.Id,
        ProjectId = placement.ProjectId,
        ProjectTitle = placement.Project.Title,
        FacultyName = placement.Project.FacultyNameSnapshot,
        StudentUserId = placement.StudentUserId,
        StudentName = placement.StudentUser.Name,
        StudentEmail = placement.StudentUser.Email,
        StudentRank = placement.StudentRank,
        FacultyRank = placement.FacultyRank,
        ResolvedByTieBreak = placement.ResolvedByTieBreak,
        Status = placement.Status,
        UpdatedAt = placement.UpdatedAt,
    };
}
