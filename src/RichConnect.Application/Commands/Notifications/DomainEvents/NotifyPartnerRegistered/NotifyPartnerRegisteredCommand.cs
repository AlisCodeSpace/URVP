using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerRegistered
{
    public class NotifyPartnerRegisteredCommand : IRequest<Unit>
    {
        public Guid PartnerId { get; set; }
    }
}
