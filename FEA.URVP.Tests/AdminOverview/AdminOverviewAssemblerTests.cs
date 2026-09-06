using FEA.URVP.Application.Queries.AdminOverview;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Tests.AdminOverview;

public sealed class AdminOverviewAssemblerTests
{
    private static readonly DateTime Now = new(2026, 9, 6, 16, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Empty_snapshot_asks_admin_to_start_a_semester()
    {
        var dto = AdminOverviewAssembler.FromSnapshot(Empty());

        Assert.Null(dto.Semester);
        Assert.Contains(dto.Attention, item => item.Id == "no-active-semester");
        Assert.Equal(0, dto.Accounts.Students);
        Assert.Equal(4, dto.Pipeline.Count);
        Assert.All(dto.Pipeline, step => Assert.Equal(0, step.Count));
    }

    [Fact]
    public void Open_application_window_is_a_warning()
    {
        var snapshot = Empty() with { ActiveSemester = ActiveSemester(windowOpen: true) };

        var attention = AdminOverviewAssembler.FromSnapshot(snapshot).Attention;

        Assert.Contains(attention, item => item.Id == "window-open" && item.Severity == "warning");
        Assert.DoesNotContain(attention, item => item.Id == "no-active-semester");
    }

    [Fact]
    public void Draft_run_and_matching_blockers_surface_as_warnings()
    {
        var semester = ActiveSemester(windowOpen: false);
        var snapshot = Empty() with
        {
            ActiveSemester = semester,
            LatestRun = DraftRun(semester),
            ProjectsWithApplicantsNoFacultyRanks = 2,
            UnreachableStudents = 4,
            StudentsWithoutProfile = 3,
            OpenProjectsWithoutStudentRanks = 1,
        };

        var attention = AdminOverviewAssembler.FromSnapshot(snapshot).Attention;
        var ids = attention.Select(item => item.Id).ToList();

        Assert.Equal(
            [
                "draft-awaiting-review",
                "projects-missing-faculty-ranks",
                "unreachable-students",
                "students-without-profile",
                "open-projects-without-ranks",
            ],
            ids);
        Assert.Equal("warning", attention.First(item => item.Id == "draft-awaiting-review").Severity);
        Assert.Equal("info", attention.First(item => item.Id == "students-without-profile").Severity);
    }

    [Fact]
    public void Pipeline_and_kpis_use_saved_profiles_open_seats_and_confirmed_placements()
    {
        var semester = ActiveSemester(windowOpen: false);
        var snapshot = Empty() with
        {
            ActiveSemester = semester,
            Students = 10,
            StudentProfiles = 7,
            ProfilesReady = 5,
            Faculty = 4,
            FacultyWithProjects = 2,
            OpenProjects = 6,
            SeatsRequired = 20,
            SeatsFilled = 8,
            StudentsWithRank = 5,
            StudentsWithFullSlate = 2,
            StudentRankingRows = 11,
            ConfirmedPlacements = 8,
            LatestRun = DraftRun(semester),
        };

        var dto = AdminOverviewAssembler.FromSnapshot(snapshot);

        Assert.Equal("Fall 2026–27", dto.Semester?.Name);
        Assert.Equal(7, dto.Pipeline.Single(s => s.Id == "profiles").Count);
        Assert.Equal(6, dto.Pipeline.Single(s => s.Id == "projects").Count);
        Assert.Equal(5, dto.Pipeline.Single(s => s.Id == "rankings").Count);
        Assert.Equal(8, dto.Pipeline.Single(s => s.Id == "placements").Count);
        Assert.Equal(12, dto.Projects.SeatsRemaining);
        Assert.Equal(MatchingRunStatus.Draft, dto.Matching.LatestRun?.Status);
        Assert.Contains("5 meet credits", dto.Pipeline.Single(s => s.Id == "profiles").Note);
    }

    [Fact]
    public void Recent_events_are_newest_first_with_relative_meta()
    {
        var snapshot = Empty() with
        {
            RecentEvents =
            [
                new AdminOverviewRecentEvent("Projects", "New project posted — Optics", Now.AddHours(-3)),
                new AdminOverviewRecentEvent("Matching", "Draft matching run for Fall 2026–27", Now.AddMinutes(-12)),
            ],
        };

        var activity = AdminOverviewAssembler.FromSnapshot(snapshot).RecentActivity;

        Assert.Equal(2, activity.Count);
        Assert.StartsWith("Draft matching run", activity[0].Text);
        Assert.Contains("min ago", activity[0].Meta);
        Assert.Contains("hr ago", activity[1].Meta);
    }

    private static AdminOverviewSnapshot Empty() => new()
    {
        UtcNow = Now,
    };

    private static Semester ActiveSemester(bool windowOpen) => new()
    {
        Name = "Fall 2026–27",
        IsActive = true,
        CycleStart = Now.AddDays(-20),
        CycleEnd = Now.AddDays(80),
        ApplicationWindowStart = Now.AddDays(-10),
        ApplicationWindowEnd = windowOpen ? Now.AddDays(10) : Now.AddDays(-1),
    };

    private static MatchingRun DraftRun(Semester semester) => new()
    {
        SemesterId = semester.Id,
        Semester = semester,
        Status = MatchingRunStatus.Draft,
        AlgorithmVersion = "da-student-proposing/v1",
        Seed = 1,
        StudentsConsidered = 10,
        StudentsMatched = 4,
        Warnings = ["The student application window is still open; rankings may change after this run."],
        CreatedByUserId = Guid.NewGuid(),
        CreatedAt = Now.AddHours(-1),
    };
}
