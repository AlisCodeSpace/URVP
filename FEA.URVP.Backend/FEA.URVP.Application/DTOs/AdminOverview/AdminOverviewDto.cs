using FEA.URVP.Application.DTOs.Matching;
using FEA.URVP.Application.DTOs.Semesters;

namespace FEA.URVP.Application.DTOs.AdminOverview;

public sealed class AdminOverviewDto
{
    public SemesterDto? Semester { get; init; }

    public AdminOverviewAccountsDto Accounts { get; init; } = new();

    public AdminOverviewProjectsDto Projects { get; init; } = new();

    public AdminOverviewRankingsDto Rankings { get; init; } = new();

    public AdminOverviewMatchingDto Matching { get; init; } = new();

    public AdminOverviewCatalogDto Catalog { get; init; } = new();

    public IReadOnlyList<AdminOverviewPipelineStepDto> Pipeline { get; init; } = [];

    public IReadOnlyList<AdminOverviewAttentionItemDto> Attention { get; init; } = [];

    public IReadOnlyList<AdminOverviewActivityItemDto> RecentActivity { get; init; } = [];
}

public sealed class AdminOverviewAccountsDto
{
    public int Students { get; init; }

    public int StudentProfiles { get; init; }

    public int StudentsWithoutProfile { get; init; }

    public int ProfilesReady { get; init; }

    public int Faculty { get; init; }

    public int FacultyWithProjects { get; init; }

    public int Admins { get; init; }
}

public sealed class AdminOverviewProjectsDto
{
    public int Open { get; init; }

    public int Matching { get; init; }

    public int Closed { get; init; }

    public int SeatsRequired { get; init; }

    public int SeatsFilled { get; init; }

    public int SeatsRemaining { get; init; }

    public int FullOpenProjects { get; init; }

    public int OpenWithoutStudentRanks { get; init; }

    public int ApplicantsWithoutFacultyRanks { get; init; }
}

public sealed class AdminOverviewRankingsDto
{
    public int StudentRankingRows { get; init; }

    public int StudentsWithRank { get; init; }

    public int StudentsWithFullSlate { get; init; }

    public int UnreachableStudents { get; init; }
}

public sealed class AdminOverviewMatchingDto
{
    public MatchingRunDto? LatestRun { get; init; }

    public int ConfirmedPlacements { get; init; }

    public int DeclinedPlacements { get; init; }

    public int CancelledPlacements { get; init; }
}

public sealed class AdminOverviewCatalogDto
{
    public int ResearchInterests { get; init; }

    public int ResearchActivityTypes { get; init; }

    public int Workshops { get; init; }

    public int News { get; init; }
}

public sealed class AdminOverviewPipelineStepDto
{
    public string Id { get; init; } = null!;

    public string Label { get; init; } = null!;

    public int Count { get; init; }

    public string Note { get; init; } = null!;
}

public sealed class AdminOverviewAttentionItemDto
{
    public string Id { get; init; } = null!;

    public string Text { get; init; } = null!;

    public string Href { get; init; } = null!;

    public string Severity { get; init; } = "info";
}

public sealed class AdminOverviewActivityItemDto
{
    public string Id { get; init; } = null!;

    public string Text { get; init; } = null!;

    public string Meta { get; init; } = null!;

    public DateTime At { get; init; }
}
