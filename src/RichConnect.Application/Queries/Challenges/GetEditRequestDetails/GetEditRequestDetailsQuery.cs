using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Queries.GetEditRequestDetails
{
    /// <summary>
    /// Query to get detailed information about a challenge edit request
    /// </summary>
    public class GetEditRequestDetailsQuery : IRequest<ChallengeEditRequestDto?>
    {
        public Guid EditRequestId { get; set; }

        public GetEditRequestDetailsQuery(Guid editRequestId)
        {
            EditRequestId = editRequestId;
        }
    }
}
