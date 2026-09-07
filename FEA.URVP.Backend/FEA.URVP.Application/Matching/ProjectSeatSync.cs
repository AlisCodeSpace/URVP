using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Enums;

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
            if (project.VolunteersFilled > 0 && project.Status == ProjectStatus.Open)
            {
                project.Status = ProjectStatus.Matching;
            }
            else if (project.VolunteersFilled == 0 && project.Status == ProjectStatus.Matching)
            {
                project.Status = ProjectStatus.Open;
            }

            project.UpdatedAt = DateTime.UtcNow;
        }
    }
}
