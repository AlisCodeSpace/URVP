using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.AdminOverview;
using FEA.URVP.Domain.Entities.ProjectRankings;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class AdminOverviewReadRepository : IAdminOverviewReadRepository
{
    private const int RecentEventLimit = 6;

    private readonly AppDbContext _db;
    private readonly ISemesterRepository _semesters;

    public AdminOverviewReadRepository(AppDbContext db, ISemesterRepository semesters)
    {
        _db = db;
        _semesters = semesters;
    }

    public async Task<AdminOverviewSnapshot> GetSnapshotAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var semester = await _semesters.FindActiveAsync(cancellationToken);

        var roleCounts = await _db.Users.AsNoTracking()
            .GroupBy(u => u.Role)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int CountRole(UserRole role) =>
            roleCounts.FirstOrDefault(r => r.Role == role)?.Count ?? 0;

        var students = CountRole(UserRole.Student);
        var faculty = CountRole(UserRole.Faculty);
        var admins = CountRole(UserRole.Admin);

        var facultyWithProjects = await _db.Projects.AsNoTracking()
            .Select(p => p.CreatedByUserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var studentProfiles = await _db.StudentProfiles.AsNoTracking()
            .CountAsync(cancellationToken);

        var studentsWithoutProfile = await _db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Student)
            .Where(u => !_db.StudentProfiles.Any(p => p.UserId == u.Id))
            .CountAsync(cancellationToken);

        var profilesReady = await _db.StudentProfiles.AsNoTracking()
            .CountAsync(
                p => p.CompletedCredits && p.CumulativeAverage >= StudentProfile.MinimumCumulativeAverage,
                cancellationToken);

        var projectGroups = await _db.Projects.AsNoTracking()
            .GroupBy(p => p.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                SeatsRequired = g.Sum(p => p.VolunteersRequired),
                SeatsFilled = g.Sum(p => p.VolunteersFilled),
            })
            .ToListAsync(cancellationToken);

        var open = projectGroups.FirstOrDefault(g => g.Status == ProjectStatus.Open);
        var matching = projectGroups.FirstOrDefault(g => g.Status == ProjectStatus.Matching);
        var closed = projectGroups.FirstOrDefault(g => g.Status == ProjectStatus.Closed);

        var fullOpenProjects = await _db.Projects.AsNoTracking()
            .CountAsync(
                p => p.Status == ProjectStatus.Open && p.VolunteersFilled >= p.VolunteersRequired,
                cancellationToken);

        var studentRankingRows = await _db.ProjectRankings.AsNoTracking()
            .CountAsync(cancellationToken);

        var studentsWithRank = await _db.ProjectRankings.AsNoTracking()
            .Select(r => r.StudentUserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var studentsWithFullSlate = await _db.ProjectRankings.AsNoTracking()
            .GroupBy(r => r.StudentUserId)
            .Select(g => g.Count())
            .CountAsync(count => count >= ProjectRanking.MaxRank, cancellationToken);

        var openProjects = await _db.Projects.AsNoTracking()
            .Where(p => p.Status == ProjectStatus.Open)
            .Select(p => new
            {
                p.Id,
                Remaining = p.VolunteersRequired - p.VolunteersFilled,
            })
            .ToListAsync(cancellationToken);

        var openIds = openProjects.Select(p => p.Id).ToList();
        var studentPairs = openIds.Count == 0
            ? []
            : await _db.ProjectRankings.AsNoTracking()
                .Where(r => openIds.Contains(r.ProjectId))
                .Select(r => new { r.ProjectId, r.StudentUserId })
                .ToListAsync(cancellationToken);

        var facultyPairs = openIds.Count == 0
            ? []
            : await _db.FacultyCandidateRankings.AsNoTracking()
                .Where(r => openIds.Contains(r.ProjectId))
                .Select(r => new { r.ProjectId, r.StudentUserId })
                .ToListAsync(cancellationToken);

        var projectsWithStudents = studentPairs.Select(r => r.ProjectId).ToHashSet();
        var projectsWithFaculty = facultyPairs.Select(r => r.ProjectId).ToHashSet();
        var rankedByFaculty = facultyPairs
            .Select(r => (r.ProjectId, r.StudentUserId))
            .ToHashSet();

        var openWithoutStudentRanks = openProjects.Count(p =>
            p.Remaining > 0 && !projectsWithStudents.Contains(p.Id));

        var applicantsWithoutFacultyRanks = openProjects.Count(p =>
            p.Remaining > 0
            && projectsWithStudents.Contains(p.Id)
            && !projectsWithFaculty.Contains(p.Id));

        var unreachableStudents = studentPairs
            .GroupBy(r => r.StudentUserId)
            .Count(g => g.All(r => !rankedByFaculty.Contains((r.ProjectId, r.StudentUserId))));

        var latestRun = semester is null
            ? null
            : await _db.MatchingRuns.AsNoTracking()
                .Include(r => r.Semester)
                .Where(r => r.SemesterId == semester.Id)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        var placementCounts = semester is null
            ? []
            : await _db.Placements.AsNoTracking()
                .Where(p => p.MatchingRun.SemesterId == semester.Id)
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

        int PlacementCount(PlacementStatus status) =>
            placementCounts.FirstOrDefault(p => p.Status == status)?.Count ?? 0;

        var interestCount = await _db.ValueListItems.AsNoTracking()
            .CountAsync(
                x => x.Kind == ValueListKind.ResearchInterest && x.IsActive,
                cancellationToken);

        var activityTypeCount = await _db.ValueListItems.AsNoTracking()
            .CountAsync(
                x => x.Kind == ValueListKind.ResearchActivityType && x.IsActive,
                cancellationToken);

        var workshopCount = await _db.Workshops.AsNoTracking().CountAsync(cancellationToken);
        var newsCount = await _db.NewsArticles.AsNoTracking().CountAsync(cancellationToken);

        var recentEvents = await LoadRecentEventsAsync(cancellationToken);

        return new AdminOverviewSnapshot
        {
            UtcNow = now,
            ActiveSemester = semester,
            Students = students,
            Faculty = faculty,
            Admins = admins,
            FacultyWithProjects = facultyWithProjects,
            StudentProfiles = studentProfiles,
            StudentsWithoutProfile = studentsWithoutProfile,
            ProfilesReady = profilesReady,
            OpenProjects = open?.Count ?? 0,
            MatchingProjects = matching?.Count ?? 0,
            ClosedProjects = closed?.Count ?? 0,
            SeatsRequired = open?.SeatsRequired ?? 0,
            SeatsFilled = open?.SeatsFilled ?? 0,
            FullOpenProjects = fullOpenProjects,
            OpenProjectsWithoutStudentRanks = openWithoutStudentRanks,
            ProjectsWithApplicantsNoFacultyRanks = applicantsWithoutFacultyRanks,
            StudentRankingRows = studentRankingRows,
            StudentsWithRank = studentsWithRank,
            StudentsWithFullSlate = studentsWithFullSlate,
            UnreachableStudents = unreachableStudents,
            LatestRun = latestRun,
            ConfirmedPlacements = PlacementCount(PlacementStatus.Confirmed),
            DeclinedPlacements = PlacementCount(PlacementStatus.Declined),
            CancelledPlacements = PlacementCount(PlacementStatus.Cancelled),
            ActiveResearchInterests = interestCount,
            ActiveActivityTypes = activityTypeCount,
            WorkshopCount = workshopCount,
            NewsCount = newsCount,
            RecentEvents = recentEvents,
        };
    }

    private async Task<IReadOnlyList<AdminOverviewRecentEvent>> LoadRecentEventsAsync(
        CancellationToken cancellationToken)
    {
        var projects = await _db.Projects.AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(3)
            .Select(p => new { p.Title, p.CreatedAt })
            .ToListAsync(cancellationToken);

        var runs = await _db.MatchingRuns.AsNoTracking()
            .Include(r => r.Semester)
            .OrderByDescending(r => r.CreatedAt)
            .Take(2)
            .Select(r => new { r.Status, SemesterName = r.Semester.Name, r.CreatedAt })
            .ToListAsync(cancellationToken);

        var workshops = await _db.Workshops.AsNoTracking()
            .OrderByDescending(w => w.CreatedAt)
            .Take(2)
            .Select(w => new { w.Title, w.CreatedAt })
            .ToListAsync(cancellationToken);

        var news = await _db.NewsArticles.AsNoTracking()
            .OrderByDescending(n => n.CreatedAt)
            .Take(2)
            .Select(n => new { n.Title, n.CreatedAt })
            .ToListAsync(cancellationToken);

        var profiles = await _db.StudentProfiles.AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(2)
            .Select(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return projects
            .Select(p => new AdminOverviewRecentEvent(
                "Projects",
                $"New project posted — {p.Title}",
                p.CreatedAt))
            .Concat(runs.Select(r => new AdminOverviewRecentEvent(
                "Matching",
                $"{r.Status} matching run for {r.SemesterName}",
                r.CreatedAt)))
            .Concat(workshops.Select(w => new AdminOverviewRecentEvent(
                "Workshops",
                $"Workshop listed — {w.Title}",
                w.CreatedAt)))
            .Concat(news.Select(n => new AdminOverviewRecentEvent(
                "News",
                $"News published — {n.Title}",
                n.CreatedAt)))
            .Concat(profiles.Select(at => new AdminOverviewRecentEvent(
                "Students",
                "Student profile saved",
                at)))
            .OrderByDescending(e => e.At)
            .Take(RecentEventLimit)
            .ToList();
    }
}
