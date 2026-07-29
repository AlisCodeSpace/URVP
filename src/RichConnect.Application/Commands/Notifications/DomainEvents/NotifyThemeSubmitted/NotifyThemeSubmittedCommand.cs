using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeSubmitted
{
    public class NotifyThemeSubmittedCommand : IRequest<Unit>
    {
        public Guid ThemeId { get; set; }
        public Guid SubmittedByUserId { get; set; }
    }
}
