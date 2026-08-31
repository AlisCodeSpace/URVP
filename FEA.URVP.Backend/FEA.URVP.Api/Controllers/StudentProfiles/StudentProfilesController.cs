using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.StudentProfiles.Upsert;
using FEA.URVP.Application.Queries.StudentProfiles.GetByUserId;
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

    /// <summary>
    /// Faculty (or admin) view of a student who ranked one of the caller's projects.
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var profile = await _mediator.Send(
            new GetStudentProfileByUserIdQuery(currentUserId, userId),
            cancellationToken);
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
