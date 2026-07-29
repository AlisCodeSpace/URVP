using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectRejected
{
    public class NotifyRDProjectRejectedCommand : IRequest<Unit>
    {
        public Guid RDProjectId { get; set; }
        public Guid RejectedByUserId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }
}
