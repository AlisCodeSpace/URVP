using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Commands.ApproveEditRequest
{
    /// <summary>
    /// Command to approve a challenge edit request
    /// Admin can approve edit requests with optional response
    /// </summary>
    public class ApproveEditRequestCommand : IRequest<ChallengeEditRequestDto>
    {
        public Guid EditRequestId { get; set; }
        public string? AdminResponse { get; set; }
        public Guid AdminId { get; set; }

        public ApproveEditRequestCommand(
            Guid editRequestId,
            string? adminResponse,
            Guid adminId)
        {
            EditRequestId = editRequestId;
            AdminResponse = adminResponse;
            AdminId = adminId;
        }
    }
}
