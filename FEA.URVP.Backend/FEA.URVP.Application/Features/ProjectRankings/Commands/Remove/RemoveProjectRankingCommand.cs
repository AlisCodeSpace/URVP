using MediatR;

namespace FEA.URVP.Application.Commands.ProjectRankings.Remove;

public sealed class RemoveProjectRankingCommand : IRequest<Unit>
{
    public Guid ProjectId { get; }
    public Guid CurrentUserId { get; set; }

    public RemoveProjectRankingCommand(Guid projectId)
    {
        ProjectId = projectId;
    }
}
