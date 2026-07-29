using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyChallengeSubmitted
{
    public class NotifyChallengeSubmittedCommand : IRequest<Unit>
    {
        public Guid ChallengeId { get; set; }
        public Guid SubmittedByUserId { get; set; }
    }
}
