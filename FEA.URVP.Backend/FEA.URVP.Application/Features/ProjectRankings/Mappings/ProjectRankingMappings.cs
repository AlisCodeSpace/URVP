using FEA.URVP.Application.DTOs.ProjectRankings;
using FEA.URVP.Domain.Entities.FacultyCandidateRankings;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.ProjectRankings;

namespace FEA.URVP.Application.Mappings;

public static class ProjectRankingMappings
{
    public static ProjectRankingDto ToDto(this ProjectRanking ranking, bool isMatched = false)
    {
        var project = ranking.Project
            ?? throw new InvalidOperationException("Project navigation is required to map a ranking.");

        return new ProjectRankingDto
        {
            Id = ranking.Id,
            ProjectId = ranking.ProjectId,
            Rank = ranking.Rank,
            ProjectTitle = project.Title,
            FacultyName = project.FacultyNameSnapshot,
            FacultyAffiliation = project.AffiliationSnapshot,
            ResearchAreas = project.ResearchAreas,
            ProjectStatus = (byte)project.Status,
            IsMatched = isMatched,
            RankedAt = ranking.CreatedAt,
            UpdatedAt = ranking.UpdatedAt,
        };
    }

    public static ProjectRankingStudentDto ToStudentDto(
        this ProjectRanking ranking,
        byte? facultyRank = null,
        Guid? rankedByUserId = null,
        string? rankedByName = null,
        Guid? assignedPlacementId = null,
        Guid? assignedProjectId = null,
        string? assignedProjectTitle = null)
    {
        var student = ranking.StudentUser;

        return new ProjectRankingStudentDto
        {
            RankingId = ranking.Id,
            StudentUserId = ranking.StudentUserId,
            StudentName = student?.Name ?? "Unknown student",
            StudentEmail = student?.Email ?? string.Empty,
            StudentUserName = student?.UserName,
            Rank = ranking.Rank,
            FacultyRank = facultyRank,
            RankedByUserId = rankedByUserId,
            RankedByName = rankedByName,
            AssignedPlacementId = assignedPlacementId,
            AssignedProjectId = assignedProjectId,
            AssignedProjectTitle = assignedProjectTitle,
            RankedAt = ranking.CreatedAt,
            UpdatedAt = ranking.UpdatedAt,
        };
    }

    public static IReadOnlyList<ProjectRankingStudentDto> ToStudentDtos(
        this IEnumerable<ProjectRanking> rankings,
        IEnumerable<FacultyCandidateRanking> facultyRanks,
        IReadOnlyDictionary<Guid, Placement>? assignmentsByStudent = null)
    {
        var facultyByStudent = facultyRanks.ToDictionary(r => r.StudentUserId);

        return rankings
            .Select(r =>
            {
                Placement? assignment = null;
                assignmentsByStudent?.TryGetValue(r.StudentUserId, out assignment);
                facultyByStudent.TryGetValue(r.StudentUserId, out var facultyRank);
                return r.ToStudentDto(
                    facultyRank?.Rank,
                    facultyRank?.RankedByUserId,
                    facultyRank?.RankedByUser?.Name,
                    assignment?.Id,
                    assignment?.ProjectId,
                    assignment?.Project?.Title);
            })
            .OrderBy(r => r.FacultyRank.HasValue ? 0 : 1)
            .ThenBy(r => r.FacultyRank)
            .ThenBy(r => r.Rank)
            .ThenBy(r => r.RankedAt)
            .ToList();
    }
}
