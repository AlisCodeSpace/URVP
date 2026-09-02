using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Matching.Confirm;
using FEA.URVP.Application.Commands.Matching.Discard;
using FEA.URVP.Application.Commands.Matching.Run;
using FEA.URVP.Application.Commands.Matching.UpdatePlacementStatus;
using FEA.URVP.Application.Queries.Matching.GetById;
using FEA.URVP.Application.Queries.Matching.List;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Matching;

/// <summary>Automatic student–project matching. Admin only.</summary>
[ApiController]
[Route("api/matching")]
[Authorize]
public sealed class MatchingController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public MatchingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List matching runs, most recent first. Optionally filter by semester.</summary>
    [HttpGet("runs")]
    public async Task<IActionResult> ListRuns(
        [FromQuery] Guid? semesterId,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        var runs = await _mediator.Send(new ListMatchingRunsQuery(semesterId), cancellationToken);
        return SuccessResponse(runs);
    }

    /// <summary>A run with its warnings and placements.</summary>
    [HttpGet("runs/{id:guid}")]
    public async Task<IActionResult> GetRun(Guid id, CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        var run = await _mediator.Send(new GetMatchingRunQuery(id), cancellationToken);
        return SuccessResponse(run);
    }

    /// <summary>
    /// Execute a dry run of the matching algorithm and save it as a draft for review.
    /// Replaces any existing draft for the semester.
    /// </summary>
    [HttpPost("runs")]
    public async Task<IActionResult> Run(
        [FromBody] RunMatchingCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return UnauthorizedResponse();

        command.CurrentUserId = userId;
        var run = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(run, "Matching run created");
    }

    /// <summary>Confirm a draft run; placements become binding and fill project seats.</summary>
    [HttpPost("runs/{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return UnauthorizedResponse();

        var run = await _mediator.Send(new ConfirmMatchingRunCommand(id, userId), cancellationToken);
        return SuccessResponse(run, "Matching run confirmed");
    }

    /// <summary>Discard a draft run.</summary>
    [HttpPost("runs/{id:guid}/discard")]
    public async Task<IActionResult> Discard(Guid id, CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        var run = await _mediator.Send(new DiscardMatchingRunCommand(id), cancellationToken);
        return SuccessResponse(run, "Matching run discarded");
    }

    /// <summary>Mark a confirmed placement as Declined or Cancelled, releasing its seat.</summary>
    [HttpPut("placements/{id:guid}/status")]
    public async Task<IActionResult> UpdatePlacementStatus(
        Guid id,
        [FromBody] UpdatePlacementStatusCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        command.PlacementId = id;
        var placement = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(placement, "Placement updated");
    }
}
