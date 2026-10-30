using MediatR;

namespace FEA.URVP.Application.Features.Projects.Commands.ResearchAssistants.Remove;

public sealed class RemoveProjectResearchAssistantCommand : IRequest<Unit>
{
    public Guid ProjectId { get; }
    public Guid AssistantId { get; }
    public Guid CurrentUserId { get; }
    public bool IsAdmin { get; }

    public RemoveProjectResearchAssistantCommand(
        Guid projectId,
        Guid assistantId,
        Guid currentUserId,
        bool isAdmin)
    {
        ProjectId = projectId;
        AssistantId = assistantId;
        CurrentUserId = currentUserId;
        IsAdmin = isAdmin;
    }
}
