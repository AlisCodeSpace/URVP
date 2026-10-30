using System.Globalization;
using FEA.URVP.Application.DTOs.AdminOverview;

namespace FEA.URVP.Application.Exports;

/// <summary>
/// One row per overview fact. Pipeline steps and attention items are left out
/// because they restate these counts and window dates.
/// </summary>
public static class OverviewExportDocuments
{
    public static readonly string[] Headers = ["Section", "Metric", "Value", "Detail"];

    private static readonly double[] Widths = [22, 52, 56, 36];

    private static readonly TimeZoneInfo AppZone = ResolveAppZone();

    public static byte[] ToExcel(AdminOverviewDto overview) =>
        ExcelWorkbook.Build("Overview", Headers, BuildRows(overview), Widths);

    private static List<IReadOnlyList<string>> BuildRows(AdminOverviewDto overview)
    {
        var rows = new List<IReadOnlyList<string>>();
        AddCycle(rows, overview);
        AddAccounts(rows, overview.Accounts);
        AddProjects(rows, overview.Projects);
        AddRankings(rows, overview.Rankings);
        AddPlacements(rows, overview.Matching);
        AddCatalog(rows, overview.Catalog);
        AddActivity(rows, overview.RecentActivity);
        return rows;
    }

    private static void AddCycle(List<IReadOnlyList<string>> rows, AdminOverviewDto overview)
    {
        var semester = overview.Semester;
        if (semester is null)
        {
            Add(rows, "Cycle", "Active cycle", "None");
            return;
        }

        Add(rows, "Cycle", "Active cycle", semester.Name.Trim());
        Add(rows, "Cycle", "Cycle dates", FormatRange(semester.CycleStart, semester.CycleEnd));
        Add(
            rows,
            "Cycle",
            "Registration window",
            FormatRange(semester.RegistrationWindowStart, semester.RegistrationWindowEnd),
            semester.IsRegistrationWindowOpen ? "Open" : "Closed");
        Add(
            rows,
            "Cycle",
            "Application window",
            FormatRange(semester.ApplicationWindowStart, semester.ApplicationWindowEnd),
            semester.IsApplicationWindowOpen ? "Open" : "Closed");
    }

    private static void AddAccounts(List<IReadOnlyList<string>> rows, AdminOverviewAccountsDto accounts)
    {
        Add(rows, "Accounts", "Students", Count(accounts.Students));
        Add(rows, "Accounts", "Student profiles", Count(accounts.StudentProfiles));
        Add(rows, "Accounts", "Students without a profile", Count(accounts.StudentsWithoutProfile));
        Add(rows, "Accounts", "Profiles meeting eligibility", Count(accounts.ProfilesReady));
        Add(rows, "Accounts", "Faculty", Count(accounts.Faculty));
        Add(rows, "Accounts", "Faculty with projects", Count(accounts.FacultyWithProjects));
        Add(rows, "Accounts", "Admins", Count(accounts.Admins));
    }

    private static void AddProjects(List<IReadOnlyList<string>> rows, AdminOverviewProjectsDto projects)
    {
        Add(rows, "Projects", "Open projects", Count(projects.Open));
        Add(rows, "Projects", "Matching projects", Count(projects.Matching));
        Add(rows, "Projects", "Closed projects", Count(projects.Closed));
        Add(rows, "Projects", "Volunteer seats required", Count(projects.SeatsRequired));
        Add(rows, "Projects", "Volunteer seats filled", Count(projects.SeatsFilled));
        Add(rows, "Projects", "Volunteer seats remaining", Count(projects.SeatsRemaining));
        Add(rows, "Projects", "Open projects at capacity", Count(projects.FullOpenProjects));
        Add(rows, "Projects", "Open projects with no student rankings", Count(projects.OpenWithoutStudentRanks));
        Add(
            rows,
            "Projects",
            "Open projects with applicants and no faculty rankings",
            Count(projects.ApplicantsWithoutFacultyRanks));
    }

    private static void AddRankings(List<IReadOnlyList<string>> rows, AdminOverviewRankingsDto rankings)
    {
        Add(rows, "Rankings", "Ranking rows", Count(rankings.StudentRankingRows));
        Add(rows, "Rankings", "Students with at least one rank", Count(rankings.StudentsWithRank));
        Add(rows, "Rankings", "Full slate of 3", Count(rankings.StudentsWithFullSlate));
        Add(rows, "Rankings", "Unreachable students", Count(rankings.UnreachableStudents));
    }

    private static void AddPlacements(List<IReadOnlyList<string>> rows, AdminOverviewMatchingDto matching)
    {
        Add(rows, "Placements", "Confirmed", Count(matching.ConfirmedPlacements));
        Add(rows, "Placements", "Declined", Count(matching.DeclinedPlacements));
        Add(rows, "Placements", "Cancelled", Count(matching.CancelledPlacements));

        if (matching.LatestRun is not { } run)
        {
            Add(rows, "Placements", "Latest matching run", "None");
            return;
        }

        var created = FormatInstant(run.CreatedAt);
        var detail = run.WarningCount > 0
            ? $"{Count(run.WarningCount)} warning{(run.WarningCount == 1 ? "" : "s")} · {created}"
            : created;
        Add(rows, "Placements", "Latest matching run", run.Status.ToString(), detail);
    }

    private static void AddCatalog(List<IReadOnlyList<string>> rows, AdminOverviewCatalogDto catalog)
    {
        Add(rows, "Catalog", "Research activity types", Count(catalog.ResearchActivityTypes));
        Add(rows, "Catalog", "Research interests", Count(catalog.ResearchInterests));
        Add(rows, "Catalog", "Workshops", Count(catalog.Workshops));
        Add(rows, "Catalog", "News", Count(catalog.News));
    }

    private static void AddActivity(
        List<IReadOnlyList<string>> rows,
        IReadOnlyList<AdminOverviewActivityItemDto> activity)
    {
        if (activity.Count == 0)
        {
            Add(rows, "Recent activity", "None", "");
            return;
        }

        foreach (var item in activity)
        {
            Add(rows, "Recent activity", item.Text, FormatInstant(item.At), ActivityKind(item.Meta));
        }
    }

    private static string ActivityKind(string meta)
    {
        const string separator = " · ";
        var index = meta.IndexOf(separator, StringComparison.Ordinal);
        return index < 0 ? meta : meta[..index];
    }

    private static void Add(
        List<IReadOnlyList<string>> rows,
        string section,
        string metric,
        string value,
        string detail = "") =>
        rows.Add([section, metric, value, detail]);

    private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string FormatRange(DateTime? start, DateTime? end)
    {
        if (start is null)
        {
            return "Not scheduled";
        }

        var from = FormatInstant(start.Value);
        return end is null ? $"{from} → open" : $"{from} → {FormatInstant(end.Value)}";
    }

    private static string FormatInstant(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, AppZone);
        return local.ToString("MMM d, yyyy, h:mm tt", CultureInfo.GetCultureInfo("en-US"));
    }

    private static TimeZoneInfo ResolveAppZone()
    {
        if (TimeZoneInfo.TryFindSystemTimeZoneById("Asia/Beirut", out var zone))
        {
            return zone;
        }

        if (TimeZoneInfo.TryFindSystemTimeZoneById("Middle East Standard Time", out zone))
        {
            return zone;
        }

        return TimeZoneInfo.Utc;
    }
}
