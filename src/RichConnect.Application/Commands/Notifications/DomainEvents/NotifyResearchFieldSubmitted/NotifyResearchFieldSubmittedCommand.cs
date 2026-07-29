using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldSubmitted
{
    public class NotifyResearchFieldSubmittedCommand : IRequest<Unit>
    {
        public Guid FieldId { get; set; }
        public Guid SubmittedByUserId { get; set; }
    }
}
