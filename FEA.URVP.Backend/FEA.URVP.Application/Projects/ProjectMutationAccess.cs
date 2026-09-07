using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Projects;

internal static class ProjectMutationAccess
{
    public static bool IsLockedForFaculty(Project project) =>
        project.Status != ProjectStatus.Open || project.VolunteersFilled > 0;

    public static void EnsureFacultyCanMutate(Project project, bool isAdmin)
    {
        if (isAdmin || !IsLockedForFaculty(project))
        {
            return;
        }

        throw new InvalidOperationException(
            "This project can no longer be edited after matching.");
    }
}
