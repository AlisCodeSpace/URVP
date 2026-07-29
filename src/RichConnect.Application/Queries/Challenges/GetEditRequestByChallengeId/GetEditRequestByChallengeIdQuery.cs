using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Queries.GetEditRequestByChallengeId
{
    /// <summary>
    /// Query to get edit request details by challenge ID (Admin only)
    /// </summary>
    public class GetEditRequestByChallengeIdQuery : IRequest<ChallengeEditRequestDto?>
    {
        public Guid ChallengeId { get; set; }

        public GetEditRequestByChallengeIdQuery(Guid challengeId)
        {
            ChallengeId = challengeId;
        }
    }
}
