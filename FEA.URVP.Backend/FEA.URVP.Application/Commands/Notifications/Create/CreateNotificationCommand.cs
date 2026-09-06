using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Commands.Notifications.Create;

public sealed record CreateNotificationCommand(
    Guid UserId,
    string Title,
    string Message,
    NotificationType Type,
    string? Link = null,
    NotificationPriority? Priority = null,
    Guid? ReferenceId = null,
    string? ReferenceType = null) : IRequest<Guid>;
