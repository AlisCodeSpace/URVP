using MediatR;

namespace FEA.URVP.Application.Features.ProjectRankings.Commands.Remove;

public sealed class RemoveProjectRankingCommand : IRequest<Unit>
{
    public Guid ProjectId { get; }
    public Guid CurrentUserId { get; set; }

    public RemoveProjectRankingCommand(Guid projectId)
    {
        ProjectId = projectId;
    }
}
