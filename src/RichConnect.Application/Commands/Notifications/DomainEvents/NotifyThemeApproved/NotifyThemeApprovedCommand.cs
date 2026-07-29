using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeApproved
{
    public class NotifyThemeApprovedCommand : IRequest<Unit>
    {
        public Guid ThemeId { get; set; }
        public Guid ApprovedByUserId { get; set; }
    }
}
