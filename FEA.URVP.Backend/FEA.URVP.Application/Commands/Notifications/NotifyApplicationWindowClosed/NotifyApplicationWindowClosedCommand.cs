using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyApplicationWindowClosed;

public sealed record NotifyApplicationWindowClosedCommand(Guid SemesterId) : IRequest<int>;

public sealed class NotifyApplicationWindowClosedCommandHandler
    : IRequestHandler<NotifyApplicationWindowClosedCommand, int>
{
    public const string ReferenceType = "Semester";

    private readonly ISemesterRepository _semesters;
    private readonly IUserRepository _users;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifyApplicationWindowClosedCommandHandler> _logger;

    public NotifyApplicationWindowClosedCommandHandler(
        ISemesterRepository semesters,
        IUserRepository users,
        IMediator mediator,
        ILogger<NotifyApplicationWindowClosedCommandHandler> logger)
    {
        _semesters = semesters;
        _users = users;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(
        NotifyApplicationWindowClosedCommand request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindByIdAsync(request.SemesterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Semester {request.SemesterId} was not found.");

        var students = await _users.ListUserIdsByRolesAsync([UserRole.Student], cancellationToken);
        return await NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            students,
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Semesters.ApplicationWindowClosedTitle(),
                NotificationMessages.Semesters.ApplicationWindowClosedMessage(semester.Name),
                NotificationType.ApplicationWindowClosed,
                NotificationLinks.StudentProjects,
                NotificationPriority.Medium,
                semester.Id,
                ReferenceType),
            cancellationToken);
    }
}
