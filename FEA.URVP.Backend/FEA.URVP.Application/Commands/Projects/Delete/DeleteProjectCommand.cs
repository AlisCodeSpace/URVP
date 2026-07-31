using MediatR;

namespace FEA.URVP.Application.Commands.Projects.Delete;

public sealed class DeleteProjectCommand : IRequest<Unit>
{
    public Guid ProjectId { get; }
    public Guid CurrentUserId { get; }
    public bool IsAdmin { get; }

    public DeleteProjectCommand(Guid projectId, Guid currentUserId, bool isAdmin)
    {
        ProjectId = projectId;
        CurrentUserId = currentUserId;
        IsAdmin = isAdmin;
    }
}
