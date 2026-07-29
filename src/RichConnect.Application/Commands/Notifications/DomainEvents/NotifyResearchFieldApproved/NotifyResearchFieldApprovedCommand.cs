using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldApproved
{
    public class NotifyResearchFieldApprovedCommand : IRequest<Unit>
    {
        public Guid FieldId { get; set; }
        public Guid ApprovedByUserId { get; set; }
    }
}
