using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.FacultyCandidateRankings;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.ProjectRankings;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Projects;

/// <summary>
/// When an academic cycle's end date has passed, mark its projects inactive
/// and store that cycle's students so the live roster can start over later.
/// </summary>
public sealed class ProjectCycleClosure
{
    private readonly IProjectRepository _projects;
    private readonly IProjectRankingRepository _studentRankings;
    private readonly IFacultyCandidateRankingRepository _facultyRankings;
    private readonly IMatchingRunRepository _runs;
    private readonly IProjectAlumniRepository _alumni;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectCycleClosure(
        IProjectRepository projects,
        IProjectRankingRepository studentRankings,
        IFacultyCandidateRankingRepository facultyRankings,
        IMatchingRunRepository runs,
        IProjectAlumniRepository alumni,
        IUnitOfWork unitOfWork)
    {
        _projects = projects;
        _studentRankings = studentRankings;
        _facultyRankings = facultyRankings;
        _runs = runs;
        _alumni = alumni;
        _unitOfWork = unitOfWork;
    }

    public async Task DeactivateEndedCyclesAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var projects = await _projects.ListTrackedOnEndedCyclesAsync(now, cancellationToken);
        if (projects.Count == 0)
        {
            return;
        }

        foreach (var project in projects)
        {
            await ArchiveStudentsAsync(project, now, cancellationToken);
            await _studentRankings.RemoveAllForProjectAsync(project.Id, cancellationToken);
            await _facultyRankings.RemoveAllForProjectAsync(project.Id, cancellationToken);
            project.Status = ProjectStatus.Inactive;
            project.UpdatedAt = now;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ArchiveStudentsAsync(
        Project project,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (await _alumni.AnyForCycleAsync(project.Id, project.SemesterId, cancellationToken))
        {
            return;
        }

        var rankings = await _studentRankings.ListByProjectAsync(project.Id, cancellationToken);
        var facultyRanks = await _facultyRankings.ListByProjectAsync(project.Id, cancellationToken);
        var placements = await _runs.ListConfirmedByProjectAsync(project.Id, cancellationToken);

        var rows = new Dictionary<Guid, ProjectAlumni>();
        foreach (var ranking in rankings)
        {
            rows[ranking.StudentUserId] = FromRanking(project, ranking, utcNow);
        }

        foreach (var facultyRank in facultyRanks)
        {
            if (rows.TryGetValue(facultyRank.StudentUserId, out var existing))
            {
                existing.FacultyRank ??= facultyRank.Rank;
                continue;
            }

            rows[facultyRank.StudentUserId] = FromFacultyRank(project, facultyRank, utcNow);
        }

        foreach (var placement in placements)
        {
            if (!rows.TryGetValue(placement.StudentUserId, out var existing))
            {
                rows[placement.StudentUserId] = FromPlacement(project, placement, utcNow);
                continue;
            }

            existing.WasConfirmed = true;
            existing.StudentRank ??= RankOrNull(placement.StudentRank);
            existing.FacultyRank ??= RankOrNull(placement.FacultyRank);
        }

        if (rows.Count > 0)
        {
            _alumni.AddRange(rows.Values);
        }
    }

    private static ProjectAlumni FromRanking(Project project, ProjectRanking ranking, DateTime utcNow) =>
        new()
        {
            ProjectId = project.Id,
            SemesterId = project.SemesterId,
            SemesterName = CycleName(project),
            StudentUserId = ranking.StudentUserId,
            StudentName = Clip(ranking.StudentUser.Name, 128),
            StudentEmail = Clip(ranking.StudentUser.Email, 256),
            StudentRank = ranking.Rank,
            WasConfirmed = false,
            ArchivedAt = utcNow
        };

    private static ProjectAlumni FromFacultyRank(
        Project project,
        FacultyCandidateRanking ranking,
        DateTime utcNow) =>
        new()
        {
            ProjectId = project.Id,
            SemesterId = project.SemesterId,
            SemesterName = CycleName(project),
            StudentUserId = ranking.StudentUserId,
            StudentName = Clip(ranking.StudentUser.Name, 128),
            StudentEmail = Clip(ranking.StudentUser.Email, 256),
            FacultyRank = ranking.Rank,
            WasConfirmed = false,
            ArchivedAt = utcNow
        };

    private static ProjectAlumni FromPlacement(Project project, Placement placement, DateTime utcNow) =>
        new()
        {
            ProjectId = project.Id,
            SemesterId = project.SemesterId,
            SemesterName = CycleName(project),
            StudentUserId = placement.StudentUserId,
            StudentName = Clip(placement.StudentUser.Name, 128),
            StudentEmail = Clip(placement.StudentUser.Email, 256),
            StudentRank = RankOrNull(placement.StudentRank),
            FacultyRank = RankOrNull(placement.FacultyRank),
            WasConfirmed = true,
            ArchivedAt = utcNow
        };

    private static string CycleName(Project project) =>
        string.IsNullOrWhiteSpace(project.Semester?.Name)
            ? "Previous cycle"
            : Clip(project.Semester.Name, 256);

    private static byte? RankOrNull(byte rank) => rank == 0 ? null : rank;

    private static string Clip(string? value, int max)
    {
        var text = value?.Trim() ?? "";
        return text.Length <= max ? text : text[..max];
    }
}
