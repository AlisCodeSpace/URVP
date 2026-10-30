using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifySemesterCycleStarted;

public sealed record NotifySemesterCycleStartedCommand(Guid SemesterId) : IRequest<int>;

public sealed class NotifySemesterCycleStartedCommandHandler
    : IRequestHandler<NotifySemesterCycleStartedCommand, int>
{
    public const string ReferenceType = "Semester";

    private readonly ISemesterRepository _semesters;
    private readonly IUserRepository _users;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifySemesterCycleStartedCommandHandler> _logger;

    public NotifySemesterCycleStartedCommandHandler(
        ISemesterRepository semesters,
        IUserRepository users,
        IMediator mediator,
        ILogger<NotifySemesterCycleStartedCommandHandler> logger)
    {
        _semesters = semesters;
        _users = users;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(
        NotifySemesterCycleStartedCommand request,
        CancellationToken cancellationToken)
    {
        var semester = await _semesters.FindByIdAsync(request.SemesterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Semester {request.SemesterId} was not found.");

        var recipients = await _users.ListUserIdsByRolesAsync(
            [UserRole.Student, UserRole.Faculty],
            cancellationToken);

        return await NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            recipients,
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Semesters.SemesterCycleStartedTitle(),
                NotificationMessages.Semesters.SemesterCycleStartedMessage(semester.Name),
                NotificationType.SemesterCycleStarted,
                NotificationLinks.Projects,
                NotificationPriority.Medium,
                semester.Id,
                ReferenceType),
            cancellationToken);
    }
}
