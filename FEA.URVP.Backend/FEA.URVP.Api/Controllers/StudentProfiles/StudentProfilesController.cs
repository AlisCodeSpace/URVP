using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.StudentProfiles.Upsert;
using FEA.URVP.Application.Queries.StudentProfiles.GetMine;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.StudentProfiles;

[ApiController]
[Route("api/student-profiles")]
[Authorize]
public sealed class StudentProfilesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public StudentProfilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get the signed-in student's profile (empty shell when not yet saved).</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var profile = await _mediator.Send(new GetMyStudentProfileQuery(userId), cancellationToken);
        return SuccessResponse(profile);
    }

    /// <summary>Create or update the signed-in student's profile.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpsertMine(
        [FromBody] UpsertStudentProfileCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        command.CurrentUserId = userId;
        var profile = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(profile, "Profile saved");
    }
}
