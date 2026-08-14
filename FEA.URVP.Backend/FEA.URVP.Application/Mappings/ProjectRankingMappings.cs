using FEA.URVP.Application.DTOs.ProjectRankings;
using FEA.URVP.Domain.Entities.ProjectRankings;

namespace FEA.URVP.Application.Mappings;

public static class ProjectRankingMappings
{
    public static ProjectRankingDto ToDto(this ProjectRanking ranking)
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
            RankedAt = ranking.CreatedAt,
            UpdatedAt = ranking.UpdatedAt,
        };
    }

    public static ProjectRankingStudentDto ToStudentDto(this ProjectRanking ranking)
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
            RankedAt = ranking.CreatedAt,
            UpdatedAt = ranking.UpdatedAt,
        };
    }
}
