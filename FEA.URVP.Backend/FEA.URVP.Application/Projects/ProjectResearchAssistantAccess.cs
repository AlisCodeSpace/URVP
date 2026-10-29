using FEA.URVP.Application.Abstractions.Persistence;
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

    /// <summary>
    /// Project owner, admin, or a research assistant on this project.
    /// Assistants can review and rank. They cannot edit the listing.
    /// </summary>
    public static async Task EnsureCanReviewAsync(
        Project project,
        Guid currentUserId,
        bool isAdmin,
        IUserRepository users,
        IProjectResearchAssistantRepository assistants,
        string deniedMessage,
        CancellationToken cancellationToken)
    {
        if (currentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        if (isAdmin || project.CreatedByUserId == currentUserId)
        {
            return;
        }

        if (await IsListedAsync(users, assistants, currentUserId, project.Id, cancellationToken))
        {
            return;
        }

        throw new UnauthorizedAccessException(deniedMessage);
    }

    public static async Task<bool> IsListedAsync(
        IUserRepository users,
        IProjectResearchAssistantRepository assistants,
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return false;
        }

        var user = await users.FindByIdAsync(userId, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return false;
        }

        return await assistants.IsListedOnProjectAsync(projectId, user.Email, cancellationToken);
    }
}
