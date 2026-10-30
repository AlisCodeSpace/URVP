using FEA.URVP.Application.Abstractions.Events;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Domain.Events.Users;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Users.AssignRole;

public sealed class AssignUserRoleCommandHandler
    : BaseCommandHandler<AssignUserRoleCommand, UserDto>
{
    private readonly IUserRepository _users;
    private readonly IEventBus _eventBus;

    public AssignUserRoleCommandHandler(
        ILogger<AssignUserRoleCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IUserRepository users,
        IEventBus eventBus)
        : base(logger, unitOfWork)
    {
        _users = users;
        _eventBus = eventBus;
    }

    protected override async Task<UserDto> HandleInternal(
        AssignUserRoleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"User {request.UserId} was not found.");

        if (user.Role == request.Role)
        {
            return user.ToDto();
        }

        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin)
        {
            var adminCount = await _users.CountByRoleAsync(UserRole.Admin, cancellationToken);
            if (adminCount <= 1)
            {
                throw new InvalidOperationException("Cannot demote the last remaining admin.");
            }

            if (user.Id == request.CurrentUserId)
            {
                throw new InvalidOperationException("You cannot demote your own admin account.");
            }
        }

        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Assigned role {Role} to user {UserId} by {ActorUserId}",
            user.Role,
            user.Id,
            request.CurrentUserId);

        await NotificationEventPublish.TryPublishAsync(
            _eventBus,
            new UserRoleAssignedEvent(user.Id),
            Logger,
            cancellationToken);

        return user.ToDto();
    }
}
