using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Notifications.Create;
using FEA.URVP.Application.Commands.Notifications.DeleteAll;
using FEA.URVP.Application.Commands.Notifications.MarkAllAsRead;
using FEA.URVP.Application.Commands.Notifications.MarkAsRead;
using FEA.URVP.Application.DTOs.Notifications;
using FEA.URVP.Application.Queries.Notifications.GetUnreadCount;
using FEA.URVP.Application.Queries.Notifications.GetUserNotifications;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Notifications;

[ApiController]
[Route("api/notification")]
[Authorize]
public sealed class NotificationController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, totalCount) = await _mediator.Send(
            new GetUserNotificationsQuery(
                userId,
                page,
                pageSize,
                unreadOnly ? false : null),
            cancellationToken);

        return PaginatedResponse(items, page, pageSize, totalCount);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var count = await _mediator.Send(new GetUnreadCountQuery(userId), cancellationToken);
        return SuccessResponse(new NotificationCountDto { Count = count });
    }

    [HttpPost("{id:guid}/mark-read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var marked = await _mediator.Send(new MarkAsReadCommand(id, userId), cancellationToken);
        if (!marked)
        {
            return ResourceNotFound("Notification", id);
        }

        return SuccessResponse(true, "Notification marked as read");
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var count = await _mediator.Send(new MarkAllAsReadCommand(userId), cancellationToken);
        return SuccessResponse(new NotificationCountDto { Count = count });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAll(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var count = await _mediator.Send(new DeleteAllNotificationsCommand(userId), cancellationToken);
        return SuccessResponse(new NotificationCountDto { Count = count }, "Notifications deleted");
    }

    [HttpPost("test")]
    public async Task<IActionResult> CreateTest(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var id = await _mediator.Send(
            new CreateNotificationCommand(
                userId,
                "Test notification",
                "This is a test notification.",
                NotificationType.NewsPublished,
                "/notifications"),
            cancellationToken);

        return SuccessResponse(id, "Test notification created");
    }
}
