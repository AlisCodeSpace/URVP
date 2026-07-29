using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Commands.ApproveChallenge
{
    public class ApproveChallengeCommand : IRequest<ChallengeDto>
    {
        public Guid ChallengeId { get; set; }
        public Guid AdminId { get; set; }

        public ApproveChallengeCommand(Guid challengeId, Guid adminId)
        {
            ChallengeId = challengeId;
            AdminId = adminId;
        }
    }
}
