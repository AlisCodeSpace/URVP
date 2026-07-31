using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Queries.Auth.GetAuthStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthStatusController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AuthStatusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAuthStatus()
    {
        var result = await _mediator.Send(new GetAuthStatusQuery(User));

        if (!result.IsAuthenticated && result.Error == "Internal server error")
        {
            return StatusCode(500, new { isAuthenticated = false, error = result.Error });
        }

        return Ok(new
        {
            isAuthenticated = result.IsAuthenticated,
            userId = result.UserId,
            email = result.Email,
            name = result.Name,
            userName = result.UserName,
            affiliation = result.Affiliation,
            role = result.Role,
            profileImageUrl = result.ProfileImageUrl,
            registeredAt = result.RegisteredAt,
            authenticationScheme = result.AuthenticationScheme,
            error = result.Error
        });
    }
}
