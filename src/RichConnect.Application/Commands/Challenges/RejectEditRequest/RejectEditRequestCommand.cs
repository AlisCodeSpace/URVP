using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Commands.RejectEditRequest
{
    /// <summary>
    /// Command to reject a challenge edit request
    /// Admin can reject edit requests with required response explaining the rejection
    /// </summary>
    public class RejectEditRequestCommand : IRequest<ChallengeEditRequestDto>
    {
        public Guid EditRequestId { get; set; }
        public string AdminResponse { get; set; } = null!;
        public Guid AdminId { get; set; }

        public RejectEditRequestCommand(
            Guid editRequestId,
            string adminResponse,
            Guid adminId)
        {
            EditRequestId = editRequestId;
            AdminResponse = adminResponse;
            AdminId = adminId;
        }
    }
}
