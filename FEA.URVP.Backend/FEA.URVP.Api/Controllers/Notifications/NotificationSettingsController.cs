using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Notifications.UpdateSettings;
using FEA.URVP.Application.DTOs.Notifications;
using FEA.URVP.Application.Queries.Notifications.GetSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Notifications;

[ApiController]
[Route("api/notificationsettings")]
[Authorize]
public sealed class NotificationSettingsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public NotificationSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var settings = await _mediator.Send(new GetNotificationSettingsQuery(userId), cancellationToken);
        settings ??= await _mediator.Send(
            new UpdateNotificationSettingsCommand(userId, true, true),
            cancellationToken);

        return SuccessResponse(settings);
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdateNotificationSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var settings = await _mediator.Send(
            new UpdateNotificationSettingsCommand(
                userId,
                request.EmailNotifications,
                request.InAppNotifications),
            cancellationToken);

        return SuccessResponse(settings, "Notification settings updated");
    }
}
