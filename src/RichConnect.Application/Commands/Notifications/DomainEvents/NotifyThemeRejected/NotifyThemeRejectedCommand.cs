using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeRejected
{
    public class NotifyThemeRejectedCommand : IRequest<Unit>
    {
        public Guid ThemeId { get; set; }
        public Guid RejectedByUserId { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
    }
}
