using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyThemeSubmissionConfirmation
{
    public class NotifyThemeSubmissionConfirmationCommand : IRequest<Guid>
    {
        public Guid ThemeId { get; set; }
        public Guid SubmittedByUserId { get; set; }
    }
}
