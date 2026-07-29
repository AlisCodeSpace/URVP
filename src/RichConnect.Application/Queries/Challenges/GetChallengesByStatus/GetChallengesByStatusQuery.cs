using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Queries.GetChallengesByStatus
{
    public class GetChallengesByStatusQuery : IRequest<List<ChallengeDto>>
    {
        public ChallengeStatus Status { get; set; }

        public GetChallengesByStatusQuery(ChallengeStatus status)
        {
            Status = status;
        }
    }
}
