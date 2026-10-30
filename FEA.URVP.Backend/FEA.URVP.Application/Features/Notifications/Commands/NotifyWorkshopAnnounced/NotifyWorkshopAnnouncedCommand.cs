using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Notifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Notifications.NotifyWorkshopAnnounced;

public sealed record NotifyWorkshopAnnouncedCommand(Guid WorkshopId) : IRequest<int>;

public sealed class NotifyWorkshopAnnouncedCommandHandler : IRequestHandler<NotifyWorkshopAnnouncedCommand, int>
{
    public const string ReferenceType = "Workshop";

    private readonly IWorkshopRepository _workshops;
    private readonly IUserRepository _users;
    private readonly IMediator _mediator;
    private readonly ILogger<NotifyWorkshopAnnouncedCommandHandler> _logger;

    public NotifyWorkshopAnnouncedCommandHandler(
        IWorkshopRepository workshops,
        IUserRepository users,
        IMediator mediator,
        ILogger<NotifyWorkshopAnnouncedCommandHandler> logger)
    {
        _workshops = workshops;
        _users = users;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<int> Handle(NotifyWorkshopAnnouncedCommand request, CancellationToken cancellationToken)
    {
        var workshop = await _workshops.FindByIdAsync(request.WorkshopId, cancellationToken)
            ?? throw new KeyNotFoundException($"Workshop {request.WorkshopId} was not found.");

        var recipients = await _users.ListUserIdsByRolesAsync(
            [UserRole.Student, UserRole.Faculty],
            cancellationToken);

        return await NotificationFanOut.SendAsync(
            _mediator,
            _logger,
            recipients,
            userId => new CreateNotificationCommand(
                userId,
                NotificationMessages.Workshops.WorkshopAnnouncedTitle(),
                NotificationMessages.Workshops.WorkshopAnnouncedMessage(workshop.Title),
                NotificationType.WorkshopAnnounced,
                NotificationLinks.Workshops,
                NotificationPriority.Low,
                workshop.Id,
                ReferenceType),
            cancellationToken);
    }
}
