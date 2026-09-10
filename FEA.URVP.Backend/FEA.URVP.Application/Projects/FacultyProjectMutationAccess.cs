using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Projects;

public sealed class FacultyProjectMutationAccess
{
    public const string LockedAfterMatchingMessage =
        "This project can no longer be edited after matching.";

    public const string LockedAfterRankingMessage =
        "This project can no longer be edited once a student has ranked it.";

    public const string LockedAfterWindowClosedMessage =
        "This project can no longer be edited after the application window closes.";

    public const string LockedAfterCycleEndedMessage =
        "This project can no longer be edited after the URVP cycle ends.";

    private readonly ISemesterRepository _semesters;
    private readonly IProjectRankingRepository _rankings;

    public FacultyProjectMutationAccess(
        ISemesterRepository semesters,
        IProjectRankingRepository rankings)
    {
        _semesters = semesters;
        _rankings = rankings;
    }

    public static bool IsLockedForCandidateRanking(Project project) =>
        project.Status != ProjectStatus.Open || project.VolunteersFilled > 0;

    public static void EnsureCanRankCandidates(Project project, bool isAdmin)
    {
        if (isAdmin || !IsLockedForCandidateRanking(project))
        {
            return;
        }

        throw new InvalidOperationException(LockedAfterMatchingMessage);
    }

    public async Task EnsureCanEditProjectAsync(
        Project project,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var reason = await GetEditLockReasonAsync(project, cancellationToken);
        if (isAdmin || reason is null)
        {
            return;
        }

        throw new InvalidOperationException(reason);
    }

    public async Task<string?> GetEditLockReasonAsync(
        Project project,
        CancellationToken cancellationToken)
    {
        if (IsLockedForCandidateRanking(project))
        {
            return LockedAfterMatchingMessage;
        }

        var context = await LoadCycleContextAsync(cancellationToken);
        if (context.CycleEnded)
        {
            return LockedAfterCycleEndedMessage;
        }

        if (context.ApplicationWindowEnded)
        {
            return LockedAfterWindowClosedMessage;
        }

        var counts = await _rankings.CountByProjectIdsAsync([project.Id], cancellationToken);
        if (counts.GetValueOrDefault(project.Id) > 0)
        {
            return LockedAfterRankingMessage;
        }

        return null;
    }

    public async Task<IReadOnlyDictionary<Guid, string?>> GetEditLockReasonsAsync(
        IReadOnlyList<Project> projects,
        CancellationToken cancellationToken)
    {
        if (projects.Count == 0)
        {
            return new Dictionary<Guid, string?>();
        }

        var context = await LoadCycleContextAsync(cancellationToken);
        var projectIds = projects.Select(p => p.Id).ToList();
        var rankingCounts = await _rankings.CountByProjectIdsAsync(projectIds, cancellationToken);

        return projects.ToDictionary(
            project => project.Id,
            project => ResolveEditLockReason(project, rankingCounts, context));
    }

    private async Task<CycleContext> LoadCycleContextAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var activeSemester = await _semesters.FindActiveAsync(cancellationToken);

        if (activeSemester is not null)
        {
            return new CycleContext(
                activeSemester.HasEnded(now),
                activeSemester.HasApplicationWindowEnded(now));
        }

        var semesters = await _semesters.ListAllAsync(cancellationToken);
        return new CycleContext(
            semesters.Any(semester => semester.HasEnded(now)),
            ApplicationWindowEnded: false);
    }

    private static string? ResolveEditLockReason(
        Project project,
        IReadOnlyDictionary<Guid, int> rankingCounts,
        CycleContext context)
    {
        if (IsLockedForCandidateRanking(project))
        {
            return LockedAfterMatchingMessage;
        }

        if (context.CycleEnded)
        {
            return LockedAfterCycleEndedMessage;
        }

        if (context.ApplicationWindowEnded)
        {
            return LockedAfterWindowClosedMessage;
        }

        if (rankingCounts.GetValueOrDefault(project.Id) > 0)
        {
            return LockedAfterRankingMessage;
        }

        return null;
    }

    private sealed record CycleContext(bool CycleEnded, bool ApplicationWindowEnded);
}
