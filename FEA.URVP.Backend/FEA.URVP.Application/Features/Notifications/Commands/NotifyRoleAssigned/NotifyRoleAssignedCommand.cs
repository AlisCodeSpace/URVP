using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyRoleAssigned;

public sealed record NotifyRoleAssignedCommand(Guid UserId) : IRequest<int>;

public sealed class NotifyRoleAssignedCommandHandler : IRequestHandler<NotifyRoleAssignedCommand, int>
{
    public const string ReferenceType = "User";

    private readonly IUserRepository _users;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifyRoleAssignedCommandHandler> _logger;

    public NotifyRoleAssignedCommandHandler(
        IUserRepository users,
        IMediator mediator,
        ILogger<NotifyRoleAssignedCommandHandler> logger)
    {
        _users = users;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(NotifyRoleAssignedCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} was not found.");

        return await NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            [user.Id],
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Users.RoleAssignedTitle(),
                NotificationMessages.Users.RoleAssignedMessage(UserMappings.ToLabel(user.Role)),
                NotificationType.RoleAssigned,
                NotificationLinks.Projects,
                NotificationPriority.Medium,
                user.Id,
                ReferenceType),
            cancellationToken);
    }
}
