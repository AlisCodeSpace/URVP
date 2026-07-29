using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerRejected
{
    public class NotifyPartnerRejectedCommand : IRequest<Unit>
    {
        public Guid PartnerId { get; set; }
        public Guid RejectedByUserId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }
}
