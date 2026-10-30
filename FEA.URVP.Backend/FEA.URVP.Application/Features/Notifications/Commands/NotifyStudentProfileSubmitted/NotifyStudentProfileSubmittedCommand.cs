using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyStudentProfileSubmitted;

public sealed record NotifyStudentProfileSubmittedCommand(Guid UserId, string StudentName) : IRequest<int>;

public sealed class NotifyStudentProfileSubmittedCommandHandler
    : IRequestHandler<NotifyStudentProfileSubmittedCommand, int>
{
    public const string ReferenceType = "User";

    private readonly IUserRepository _users;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifyStudentProfileSubmittedCommandHandler> _logger;

    public NotifyStudentProfileSubmittedCommandHandler(
        IUserRepository users,
        IMediator mediator,
        ILogger<NotifyStudentProfileSubmittedCommandHandler> logger)
    {
        _users = users;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(
        NotifyStudentProfileSubmittedCommand request,
        CancellationToken cancellationToken)
    {
        var admins = await _users.ListUserIdsByRolesAsync([UserRole.Admin], cancellationToken);
        return await NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            admins,
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Profiles.StudentProfileSubmittedTitle(),
                NotificationMessages.Profiles.StudentProfileSubmittedMessage(request.StudentName),
                NotificationType.StudentProfileSubmitted,
                NotificationLinks.AdminUsers,
                NotificationPriority.Low,
                request.UserId,
                ReferenceType),
            cancellationToken);
    }
}
