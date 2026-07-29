using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerCriticalUpdate
{
    public class NotifyPartnerCriticalUpdateCommand : IRequest<Unit>
    {
        public Guid PartnerId { get; set; }
        public Guid UpdatedByUserId { get; set; }
        public List<string> CriticalFieldsChanged { get; set; } = new();
    }
}
