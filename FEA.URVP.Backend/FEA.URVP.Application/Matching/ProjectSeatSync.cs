using FEA.URVP.Application.Abstractions.Persistence;

namespace FEA.URVP.Application.Matching;

/// <summary>
/// Keeps <c>Project.VolunteersFilled</c> equal to the number of confirmed placements,
/// so existing seat guards (ranking, capacity edits) reflect matching results.
/// Callers must have persisted placement changes before invoking.
/// </summary>
internal static class ProjectSeatSync
{
    public static async Task ApplyAsync(
        IEnumerable<Guid> projectIds,
        IProjectRepository projects,
        IMatchingRunRepository runs,
        CancellationToken cancellationToken)
    {
        foreach (var projectId in projectIds.Distinct())
        {
            var project = await projects.FindByIdAsync(projectId, cancellationToken);
            if (project is null) continue;

            project.VolunteersFilled = await runs.CountConfirmedByProjectAsync(projectId, cancellationToken);
            project.UpdatedAt = DateTime.UtcNow;
        }
    }
}
