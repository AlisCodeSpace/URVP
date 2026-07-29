using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Queries.GetChallengeById
{
    public class GetChallengeByIdQuery : IRequest<ChallengeDto?>
    {
        public Guid ChallengeId { get; set; }
        public Guid UserId { get; set; }
        public string UserRole { get; set; }

        public GetChallengeByIdQuery(Guid challengeId, Guid userId, string userRole)
        {
            ChallengeId = challengeId;
            UserId = userId;
            UserRole = userRole;
        }
    }
}
