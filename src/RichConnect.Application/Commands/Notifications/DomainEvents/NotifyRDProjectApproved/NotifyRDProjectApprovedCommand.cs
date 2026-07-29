using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectApproved
{
    public class NotifyRDProjectApprovedCommand : IRequest<Unit>
    {
        public Guid RDProjectId { get; set; }
        public Guid ApprovedByUserId { get; set; }
    }
}
