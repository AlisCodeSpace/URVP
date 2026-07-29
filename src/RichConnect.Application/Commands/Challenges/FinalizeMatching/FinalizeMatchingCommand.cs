using MediatR;
using RICHConnect.Backend.Application.DTOs.Matching;

namespace RICHConnect.Backend.Application.Commands.FinalizeMatching
{
    /// <summary>
    /// Command to finalize challenge matching with professors
    /// </summary>
    public class FinalizeMatchingCommand : IRequest<MatchFinalizeDto>
    {
        public Guid ChallengeId { get; set; }
        public Guid AdminId { get; set; }

        public FinalizeMatchingCommand(Guid challengeId, Guid adminId)
        {
            ChallengeId = challengeId;
            AdminId = adminId;
        }
    }
}
