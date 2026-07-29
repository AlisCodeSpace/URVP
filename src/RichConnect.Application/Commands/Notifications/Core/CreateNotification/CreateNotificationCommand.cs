using MediatR;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.Notifications.CreateNotification;

public class CreateNotificationCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? Link { get; set; }
    public string? Priority { get; set; } = "low";
    public Guid? ReferenceId { get; set; } // Links to Challenge/Partner/Theme/ResearchField
    public string? ReferenceType { get; set; } // "Challenge", "Partner", "Theme", "ResearchField"
}

