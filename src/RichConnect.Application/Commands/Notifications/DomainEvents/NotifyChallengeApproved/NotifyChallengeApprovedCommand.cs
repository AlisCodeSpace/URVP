using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyChallengeApproved
{
    public class NotifyChallengeApprovedCommand : IRequest<Unit>
    {
        public Guid ChallengeId { get; set; }
        public Guid ApprovedByUserId { get; set; }
    }
}

