using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.Semesters;

namespace FEA.URVP.Application.Queries.AdminOverview;

public sealed record AdminOverviewSnapshot
{
    public required DateTime UtcNow { get; init; }

    public Semester? ActiveSemester { get; init; }

    public int Students { get; init; }

    public int Faculty { get; init; }

    public int Admins { get; init; }

    public int FacultyWithProjects { get; init; }

    public int StudentProfiles { get; init; }

    public int StudentsWithoutProfile { get; init; }

    public int ProfilesReady { get; init; }

    public int OpenProjects { get; init; }

    public int MatchingProjects { get; init; }

    public int ClosedProjects { get; init; }

    public int SeatsRequired { get; init; }

    public int SeatsFilled { get; init; }

    public int FullOpenProjects { get; init; }

    public int OpenProjectsWithoutStudentRanks { get; init; }

    public int ProjectsWithApplicantsNoFacultyRanks { get; init; }

    public int StudentRankingRows { get; init; }

    public int StudentsWithRank { get; init; }

    public int StudentsWithFullSlate { get; init; }

    public int UnreachableStudents { get; init; }

    public MatchingRun? LatestRun { get; init; }

    public int ConfirmedPlacements { get; init; }

    public int DeclinedPlacements { get; init; }

    public int CancelledPlacements { get; init; }

    public int ActiveResearchInterests { get; init; }

    public int ActiveActivityTypes { get; init; }

    public int WorkshopCount { get; init; }

    public int NewsCount { get; init; }

    public IReadOnlyList<AdminOverviewRecentEvent> RecentEvents { get; init; } = [];
}

public sealed record AdminOverviewRecentEvent(string Kind, string Text, DateTime At);
