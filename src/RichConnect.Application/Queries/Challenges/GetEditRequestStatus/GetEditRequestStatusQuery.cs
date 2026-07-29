using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Queries.GetEditRequestStatus
{
    public class GetEditRequestStatusQuery : IRequest<ChallengeEditRequestDto?>
    {
        public Guid ChallengeId { get; set; }
        public Guid UserId { get; set; }

        public GetEditRequestStatusQuery(Guid challengeId, Guid userId)
        {
            ChallengeId = challengeId;
            UserId = userId;
        }
    }
}
