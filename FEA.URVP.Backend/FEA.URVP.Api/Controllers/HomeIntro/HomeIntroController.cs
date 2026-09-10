using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.HomeIntro.Update;
using FEA.URVP.Application.Queries.HomeIntro;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.HomeIntro;

[ApiController]
[Route("api/home-intro")]
public sealed class HomeIntroController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public HomeIntroController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var intro = await _mediator.Send(new GetHomeIntroQuery(), cancellationToken);
        return SuccessResponse(intro);
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdateHomeIntroCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        command.UpdatedByUserId = GetCurrentUserId();
        var intro = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(intro, "Home intro updated");
    }
}
