using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerApproved
{
    public class NotifyPartnerApprovedCommand : IRequest<Unit>
    {
        public Guid PartnerId { get; set; }
        public Guid ApprovedByUserId { get; set; }
    }
}
