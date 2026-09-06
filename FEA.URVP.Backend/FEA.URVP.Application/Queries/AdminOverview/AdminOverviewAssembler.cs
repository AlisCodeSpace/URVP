using FEA.URVP.Application.DTOs.AdminOverview;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Queries.AdminOverview;

public static class AdminOverviewAssembler
{
    public static AdminOverviewDto FromSnapshot(AdminOverviewSnapshot snapshot)
    {
        var now = snapshot.UtcNow;
        var semester = snapshot.ActiveSemester;
        var seatsRemaining = Math.Max(0, snapshot.SeatsRequired - snapshot.SeatsFilled);

        return new AdminOverviewDto
        {
            Semester = semester?.ToDto(),
            Accounts = new AdminOverviewAccountsDto
            {
                Students = snapshot.Students,
                StudentProfiles = snapshot.StudentProfiles,
                StudentsWithoutProfile = snapshot.StudentsWithoutProfile,
                ProfilesReady = snapshot.ProfilesReady,
                Faculty = snapshot.Faculty,
                FacultyWithProjects = snapshot.FacultyWithProjects,
                Admins = snapshot.Admins,
            },
            Projects = new AdminOverviewProjectsDto
            {
                Open = snapshot.OpenProjects,
                Matching = snapshot.MatchingProjects,
                Closed = snapshot.ClosedProjects,
                SeatsRequired = snapshot.SeatsRequired,
                SeatsFilled = snapshot.SeatsFilled,
                SeatsRemaining = seatsRemaining,
                FullOpenProjects = snapshot.FullOpenProjects,
                OpenWithoutStudentRanks = snapshot.OpenProjectsWithoutStudentRanks,
                ApplicantsWithoutFacultyRanks = snapshot.ProjectsWithApplicantsNoFacultyRanks,
            },
            Rankings = new AdminOverviewRankingsDto
            {
                StudentRankingRows = snapshot.StudentRankingRows,
                StudentsWithRank = snapshot.StudentsWithRank,
                StudentsWithFullSlate = snapshot.StudentsWithFullSlate,
                UnreachableStudents = snapshot.UnreachableStudents,
            },
            Matching = new AdminOverviewMatchingDto
            {
                LatestRun = snapshot.LatestRun is MatchingRun run ? run.ToDto() : null,
                ConfirmedPlacements = snapshot.ConfirmedPlacements,
                DeclinedPlacements = snapshot.DeclinedPlacements,
                CancelledPlacements = snapshot.CancelledPlacements,
            },
            Catalog = new AdminOverviewCatalogDto
            {
                ResearchInterests = snapshot.ActiveResearchInterests,
                ResearchActivityTypes = snapshot.ActiveActivityTypes,
                Workshops = snapshot.WorkshopCount,
                News = snapshot.NewsCount,
            },
            Pipeline = BuildPipeline(snapshot),
            Attention = BuildAttention(snapshot),
            RecentActivity = snapshot.RecentEvents
                .OrderByDescending(e => e.At)
                .Select((e, index) => new AdminOverviewActivityItemDto
                {
                    Id = $"{e.Kind}-{index}-{e.At:O}",
                    Text = e.Text,
                    Meta = $"{e.Kind} · {FormatRelative(e.At, now)}",
                    At = AsUtc(e.At),
                })
                .ToList(),
        };
    }

    internal static IReadOnlyList<AdminOverviewPipelineStepDto> BuildPipeline(
        AdminOverviewSnapshot snapshot) =>
    [
        new()
        {
            Id = "profiles",
            Label = "Profiles saved",
            Count = snapshot.StudentProfiles,
            Note = snapshot.ProfilesReady > 0
                ? $"{snapshot.ProfilesReady} meet credits + {StudentProfile.MinimumCumulativeAverage:0} average"
                : "Ready for ranking once eligibility is met",
        },
        new()
        {
            Id = "projects",
            Label = "Projects live",
            Count = snapshot.OpenProjects,
            Note = snapshot.OpenProjects > 0
                ? $"{Math.Max(0, snapshot.SeatsRequired - snapshot.SeatsFilled)} seats still open"
                : "Visible in the catalog",
        },
        new()
        {
            Id = "rankings",
            Label = "Rankings submitted",
            Count = snapshot.StudentsWithRank,
            Note = snapshot.StudentsWithFullSlate > 0
                ? $"{snapshot.StudentsWithFullSlate} with a full slate of 3"
                : "Students with at least one rank",
        },
        new()
        {
            Id = "placements",
            Label = "Matched placements",
            Count = snapshot.ConfirmedPlacements,
            Note = "Confirmed seats this semester",
        },
    ];

    internal static IReadOnlyList<AdminOverviewAttentionItemDto> BuildAttention(
        AdminOverviewSnapshot snapshot)
    {
        var items = new List<AdminOverviewAttentionItemDto>();
        var semester = snapshot.ActiveSemester;
        var now = snapshot.UtcNow;

        if (semester is null)
        {
            items.Add(new AdminOverviewAttentionItemDto
            {
                Id = "no-active-semester",
                Text = "No active semester. Start a cycle before students can apply or matching can run.",
                Href = "/admin/semesters",
                Severity = "warning",
            });
        }
        else if (semester.IsApplicationWindowOpen(now))
        {
            items.Add(new AdminOverviewAttentionItemDto
            {
                Id = "window-open",
                Text = "The student application window is still open. Rankings may change if you confirm a run now.",
                Href = "/admin/semesters",
                Severity = "warning",
            });
        }

        if (snapshot.LatestRun is { Status: MatchingRunStatus.Draft })
        {
            items.Add(new AdminOverviewAttentionItemDto
            {
                Id = "draft-awaiting-review",
                Text = "A draft matching run is waiting for review.",
                Href = "/admin/matching",
                Severity = "warning",
            });
        }

        if (snapshot.ProjectsWithApplicantsNoFacultyRanks > 0)
        {
            items.Add(new AdminOverviewAttentionItemDto
            {
                Id = "projects-missing-faculty-ranks",
                Text =
                    $"{snapshot.ProjectsWithApplicantsNoFacultyRanks} open project(s) have applicants but no faculty rankings and will be skipped.",
                Href = "/admin/projects",
                Severity = "warning",
            });
        }

        if (snapshot.UnreachableStudents > 0)
        {
            items.Add(new AdminOverviewAttentionItemDto
            {
                Id = "unreachable-students",
                Text =
                    $"{snapshot.UnreachableStudents} student(s) were not ranked by any faculty on their chosen projects and cannot be placed.",
                Href = "/admin/projects",
                Severity = "warning",
            });
        }

        if (snapshot.StudentsWithoutProfile > 0)
        {
            items.Add(new AdminOverviewAttentionItemDto
            {
                Id = "students-without-profile",
                Text = $"{snapshot.StudentsWithoutProfile} student account(s) have not saved a profile yet.",
                Href = "/admin/users",
                Severity = "info",
            });
        }

        if (snapshot.OpenProjectsWithoutStudentRanks > 0)
        {
            items.Add(new AdminOverviewAttentionItemDto
            {
                Id = "open-projects-without-ranks",
                Text =
                    $"{snapshot.OpenProjectsWithoutStudentRanks} open project(s) with remaining seats have no student rankings.",
                Href = "/admin/projects",
                Severity = "info",
            });
        }

        return items;
    }

    private static string FormatRelative(DateTime at, DateTime now)
    {
        var utc = AsUtc(at);
        var elapsed = now - utc;
        if (elapsed.TotalSeconds < 45) return "just now";
        if (elapsed.TotalMinutes < 2) return "1 min ago";
        if (elapsed.TotalMinutes < 60) return $"{(int)elapsed.TotalMinutes} min ago";
        if (elapsed.TotalHours < 2) return "1 hr ago";
        if (elapsed.TotalHours < 24) return $"{(int)elapsed.TotalHours} hr ago";
        if (elapsed.TotalDays < 2) return "Yesterday";
        if (elapsed.TotalDays < 7) return $"{(int)elapsed.TotalDays} days ago";
        return utc.ToString("MMM d, yyyy");
    }

    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
