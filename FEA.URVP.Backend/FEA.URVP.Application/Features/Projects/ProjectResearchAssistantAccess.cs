using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.FacultyCandidateRankings;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Application.Projects;

public sealed class ProjectResearchAssistantAccess
{
    private readonly IUserRepository _users;
    private readonly IProjectResearchAssistantRepository _assistants;

    public ProjectResearchAssistantAccess(
        IUserRepository users,
        IProjectResearchAssistantRepository assistants)
    {
        _users = users;
        _assistants = assistants;
    }

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
    public async Task EnsureCanReviewAsync(
        Project project,
        Guid currentUserId,
        bool isAdmin,
        string deniedMessage,
        CancellationToken cancellationToken)
    {
        if (currentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        if (!await CanReviewAsync(project, currentUserId, isAdmin, cancellationToken))
        {
            throw new UnauthorizedAccessException(deniedMessage);
        }
    }

    public async Task<bool> CanReviewAsync(
        Project project,
        Guid currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken) =>
        isAdmin
        || project.CreatedByUserId == currentUserId
        || await IsListedAsync(currentUserId, project.Id, cancellationToken);

    /// <summary>
    /// Owner or admin with a faculty/admin role, or anyone listed as an assistant on the project.
    /// </summary>
    public async Task EnsureCanRankCandidatesAsync(
        Project project,
        User user,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var assists = await _assistants.IsListedOnProjectAsync(project.Id, user.Email, cancellationToken);
        if (assists)
        {
            return;
        }

        if (!isAdmin && project.CreatedByUserId != user.Id)
        {
            throw new UnauthorizedAccessException(
                "You can only rank candidates for a project you posted or assist.");
        }

        FacultyCandidateRankingAccess.EnsureCanRank(user.Role);
    }

    private async Task<bool> IsListedAsync(
        Guid userId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return false;
        }

        var user = await _users.FindByIdAsync(userId, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return false;
        }

        return await _assistants.IsListedOnProjectAsync(projectId, user.Email, cancellationToken);
    }
}
