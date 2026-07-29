using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.DomainEvents.NotifyRDProjectSubmitted
{
    public class NotifyRDProjectSubmittedCommand : IRequest<Unit>
    {
        public Guid RDProjectId { get; set; }
        public Guid SubmittedByUserId { get; set; }
    }
}
