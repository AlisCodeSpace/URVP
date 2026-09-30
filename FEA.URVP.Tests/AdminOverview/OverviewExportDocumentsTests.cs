using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using FEA.URVP.Application.DTOs.AdminOverview;
using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Application.Exports;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Tests.AdminOverview;

public sealed class OverviewExportDocumentsTests
{
    [Fact]
    public void Excel_lists_each_overview_fact_once()
    {
        var bytes = OverviewExportDocuments.ToExcel(Sample());
        var cells = Cells(ReadSheet(bytes));

        Assert.Equal(
            [
                "Section", "Metric", "Value", "Detail",
                "Cycle", "Active cycle", "Fall 2026", "",
                "Cycle", "Cycle dates", "Sep 1, 2026, 8:00 AM → Dec 15, 2026, 4:00 PM", "",
                "Cycle", "Registration window", "Aug 15, 2026, 8:00 AM → Sep 1, 2026, 5:00 PM", "Closed",
                "Cycle", "Application window", "Sep 2, 2026, 8:00 AM → Sep 30, 2026, 5:00 PM", "Open",
                "Accounts", "Students", "10", "",
                "Accounts", "Student profiles", "8", "",
                "Accounts", "Students without a profile", "2", "",
                "Accounts", "Profiles meeting eligibility", "6", "",
                "Accounts", "Faculty", "4", "",
                "Accounts", "Faculty with projects", "3", "",
                "Accounts", "Admins", "1", "",
                "Projects", "Open projects", "5", "",
                "Projects", "Matching projects", "2", "",
                "Projects", "Closed projects", "1", "",
                "Projects", "Volunteer seats required", "20", "",
                "Projects", "Volunteer seats filled", "8", "",
                "Projects", "Volunteer seats remaining", "12", "",
                "Projects", "Open projects at capacity", "1", "",
                "Projects", "Open projects with no student rankings", "2", "",
                "Projects", "Open projects with applicants and no faculty rankings", "3", "",
                "Rankings", "Ranking rows", "14", "",
                "Rankings", "Students with at least one rank", "6", "",
                "Rankings", "Full slate of 3", "4", "",
                "Rankings", "Unreachable students", "2", "",
                "Placements", "Confirmed", "7", "",
                "Placements", "Declined", "1", "",
                "Placements", "Cancelled", "2", "",
                "Placements", "Latest matching run", "Draft", "2 warnings · Sep 6, 2026, 4:00 PM",
                "Catalog", "Research activity types", "9", "",
                "Catalog", "Research interests", "11", "",
                "Catalog", "Workshops", "4", "",
                "Catalog", "News", "3", "",
                "Recent activity", "New project posted — Harbor Study", "Sep 6, 2026, 4:00 PM", "Projects",
            ],
            cells);

        Assert.DoesNotContain("Profiles saved", cells);
        Assert.DoesNotContain("Projects live", cells);
        Assert.DoesNotContain("2 hr ago", cells);
        Assert.DoesNotContain("A draft matching run is waiting for review.", cells);
        Assert.Equal(1, cells.Count(cell => cell == "Student profiles"));
        Assert.Equal(1, cells.Count(cell => cell == "Confirmed"));
        Assert.Equal(1, cells.Count(cell => cell == "Volunteer seats filled"));
    }

    [Fact]
    public void Empty_overview_keeps_zero_counts_and_omits_blank_windows()
    {
        var cells = Cells(ReadSheet(OverviewExportDocuments.ToExcel(new AdminOverviewDto())));

        Assert.Contains("None", cells);
        Assert.Equal(1, cells.Count(cell => cell == "Active cycle"));
        Assert.DoesNotContain("Registration window", cells);
        Assert.DoesNotContain("Not scheduled", cells);
        Assert.Equal("None", cells[cells.IndexOf("Latest matching run") + 1]);
        Assert.Equal("None", cells[cells.IndexOf("Recent activity") + 1]);
        Assert.Equal(1, cells.Count(cell => cell == "Students"));
    }

    private static AdminOverviewDto Sample() => new()
    {
        Semester = new SemesterDto
        {
            Id = Guid.NewGuid(),
            Name = "Fall 2026",
            IsActive = true,
            CycleStart = new DateTime(2026, 9, 1, 5, 0, 0, DateTimeKind.Utc),
            CycleEnd = new DateTime(2026, 12, 15, 14, 0, 0, DateTimeKind.Utc),
            RegistrationWindowStart = new DateTime(2026, 8, 15, 5, 0, 0, DateTimeKind.Utc),
            RegistrationWindowEnd = new DateTime(2026, 9, 1, 14, 0, 0, DateTimeKind.Utc),
            IsRegistrationWindowOpen = false,
            ApplicationWindowStart = new DateTime(2026, 9, 2, 5, 0, 0, DateTimeKind.Utc),
            ApplicationWindowEnd = new DateTime(2026, 9, 30, 14, 0, 0, DateTimeKind.Utc),
            IsApplicationWindowOpen = true,
            CreatedAt = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
        },
        Accounts = new AdminOverviewAccountsDto
        {
            Students = 10,
            StudentProfiles = 8,
            StudentsWithoutProfile = 2,
            ProfilesReady = 6,
            Faculty = 4,
            FacultyWithProjects = 3,
            Admins = 1,
        },
        Projects = new AdminOverviewProjectsDto
        {
            Open = 5,
            Matching = 2,
            Closed = 1,
            SeatsRequired = 20,
            SeatsFilled = 8,
            SeatsRemaining = 12,
            FullOpenProjects = 1,
            OpenWithoutStudentRanks = 2,
            ApplicantsWithoutFacultyRanks = 3,
        },
        Rankings = new AdminOverviewRankingsDto
        {
            StudentRankingRows = 14,
            StudentsWithRank = 6,
            StudentsWithFullSlate = 4,
            UnreachableStudents = 2,
        },
        Matching = new AdminOverviewMatchingDto
        {
            ConfirmedPlacements = 7,
            DeclinedPlacements = 1,
            CancelledPlacements = 2,
            LatestRun = new MatchingRunDto
            {
                Id = Guid.NewGuid(),
                SemesterId = Guid.NewGuid(),
                SemesterName = "Fall 2026",
                Status = MatchingRunStatus.Draft,
                AlgorithmVersion = "v1",
                WarningCount = 2,
                CreatedAt = new DateTime(2026, 9, 6, 13, 0, 0, DateTimeKind.Utc),
            },
        },
        Catalog = new AdminOverviewCatalogDto
        {
            ResearchActivityTypes = 9,
            ResearchInterests = 11,
            Workshops = 4,
            News = 3,
        },
        Pipeline =
        [
            new AdminOverviewPipelineStepDto
            {
                Id = "profiles",
                Label = "Profiles saved",
                Count = 8,
                Note = "6 meet credits",
            },
            new AdminOverviewPipelineStepDto
            {
                Id = "projects",
                Label = "Projects live",
                Count = 5,
                Note = "12 seats still open",
            },
        ],
        Attention =
        [
            new AdminOverviewAttentionItemDto
            {
                Id = "draft-awaiting-review",
                Text = "A draft matching run is waiting for review.",
                Href = "/admin/matching",
                Severity = "warning",
            },
        ],
        RecentActivity =
        [
            new AdminOverviewActivityItemDto
            {
                Id = "projects-0",
                Text = "New project posted — Harbor Study",
                Meta = "Projects · 2 hr ago",
                At = new DateTime(2026, 9, 6, 13, 0, 0, DateTimeKind.Utc),
            },
        ],
    };

    private static string ReadSheet(byte[] xlsx)
    {
        using var zip = new ZipArchive(new MemoryStream(xlsx), ZipArchiveMode.Read);
        var entry = zip.GetEntry("xl/worksheets/sheet1.xml");
        Assert.NotNull(entry);
        using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static List<string> Cells(string sheet) =>
        Regex.Matches(sheet, @"<t xml:space=""preserve"">(.*?)</t>")
            .Select(match => match.Groups[1].Value)
            .ToList();
}
