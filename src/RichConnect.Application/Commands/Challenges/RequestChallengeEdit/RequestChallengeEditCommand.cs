using MediatR;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Commands.RequestChallengeEdit
{
    /// <summary>
    /// Command to request an edit for a submitted challenge
    /// Community Partners can request edits with a reason, which will be reviewed by admins
    /// </summary>
    public class RequestChallengeEditCommand : IRequest<ChallengeEditRequestDto>
    {
        public Guid ChallengeId { get; set; }
        public string EditReason { get; set; } = null!;
        public Guid RequestedBy { get; set; }

        public RequestChallengeEditCommand(
            Guid challengeId,
            string editReason,
            Guid requestedBy)
        {
            ChallengeId = challengeId;
            EditReason = editReason;
            RequestedBy = requestedBy;
        }
    }
}
