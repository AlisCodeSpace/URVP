using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Queries.GetUserChallenges
{
    public class GetUserChallengesQuery : IRequest<List<ChallengeDto>>
    {
        public Guid UserId { get; set; }

        public GetUserChallengesQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}
