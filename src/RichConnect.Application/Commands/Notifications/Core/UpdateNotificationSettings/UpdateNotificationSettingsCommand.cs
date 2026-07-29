using MediatR;

namespace RICHConnect.Backend.Application.Commands.Notifications.UpdateNotificationSettings;

public class UpdateNotificationSettingsCommand : IRequest<bool>
{
    public Guid UserId { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool InAppNotifications { get; set; } = true;
}

