using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Commands.RejectChallenge
{
    public class RejectChallengeCommand : IRequest<ChallengeDto>
    {
        public Guid ChallengeId { get; set; }
        public Guid AdminId { get; set; }
        public RejectChallengeDto RejectDto { get; set; }

        public RejectChallengeCommand(Guid challengeId, Guid adminId, RejectChallengeDto rejectDto)
        {
            ChallengeId = challengeId;
            AdminId = adminId;
            RejectDto = rejectDto;
        }
    }
}
