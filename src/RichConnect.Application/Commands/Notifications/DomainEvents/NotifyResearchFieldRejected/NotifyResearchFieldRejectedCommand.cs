using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldRejected
{
    public class NotifyResearchFieldRejectedCommand : IRequest<Unit>
    {
        public Guid FieldId { get; set; }
        public Guid RejectedByUserId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }
}
