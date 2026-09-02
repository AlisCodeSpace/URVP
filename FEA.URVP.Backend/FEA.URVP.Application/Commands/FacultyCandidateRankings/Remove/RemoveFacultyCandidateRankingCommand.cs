using MediatR;

namespace FEA.URVP.Application.Commands.FacultyCandidateRankings.Remove;

public sealed class RemoveFacultyCandidateRankingCommand : IRequest<Unit>
{
    public Guid ProjectId { get; }
    public Guid StudentUserId { get; }
    public Guid CurrentUserId { get; set; }
    public bool IsAdmin { get; set; }

    public RemoveFacultyCandidateRankingCommand(Guid projectId, Guid studentUserId)
    {
        ProjectId = projectId;
        StudentUserId = studentUserId;
    }
}
