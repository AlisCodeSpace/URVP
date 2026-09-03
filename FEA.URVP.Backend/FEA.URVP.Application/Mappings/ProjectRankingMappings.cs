using FEA.URVP.Application.DTOs.ProjectRankings;
using FEA.URVP.Domain.Entities.FacultyCandidateRankings;
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
        byte? facultyRank = null)
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
            RankedAt = ranking.CreatedAt,
            UpdatedAt = ranking.UpdatedAt,
        };
    }

    public static IReadOnlyList<ProjectRankingStudentDto> ToStudentDtos(
        this IEnumerable<ProjectRanking> rankings,
        IEnumerable<FacultyCandidateRanking> facultyRanks)
    {
        var facultyByStudent = facultyRanks.ToDictionary(r => r.StudentUserId, r => r.Rank);

        return rankings
            .Select(r => r.ToStudentDto(
                facultyByStudent.TryGetValue(r.StudentUserId, out var facultyRank) ? facultyRank : null))
            .OrderBy(r => r.FacultyRank.HasValue ? 0 : 1)
            .ThenBy(r => r.FacultyRank)
            .ThenBy(r => r.Rank)
            .ThenBy(r => r.RankedAt)
            .ToList();
    }
}
