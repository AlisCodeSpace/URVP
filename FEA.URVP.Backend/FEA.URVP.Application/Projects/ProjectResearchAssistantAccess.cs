using FEA.URVP.Domain.Entities.Projects;

namespace FEA.URVP.Application.Projects;

internal static class ProjectResearchAssistantAccess
{
    public static void EnsureCanManage(Project project, Guid currentUserId, bool isAdmin)
    {
        if (currentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        if (!isAdmin && project.CreatedByUserId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                "You can only manage research assistants on your own projects.");
        }
    }
}
