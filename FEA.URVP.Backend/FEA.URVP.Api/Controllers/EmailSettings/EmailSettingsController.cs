using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Email.Update;
using FEA.URVP.Application.Queries.Email;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.EmailSettings;

[ApiController]
[Route("api/admin/email-settings")]
[Authorize]
public sealed class EmailSettingsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public EmailSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        var settings = await _mediator.Send(new GetEmailSettingsQuery(), cancellationToken);
        return SuccessResponse(settings);
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdateEmailSettingsCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        command.UpdatedByUserId = GetCurrentUserId();
        var settings = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(settings, "Email settings updated");
    }
}
