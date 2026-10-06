using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Matching.Assign;
using FEA.URVP.Application.Commands.Matching.Confirm;
using FEA.URVP.Application.Commands.Matching.Discard;
using FEA.URVP.Application.Commands.Matching.Run;
using FEA.URVP.Application.Commands.Matching.UpdatePlacementStatus;
using FEA.URVP.Application.Queries.Matching.GetById;
using FEA.URVP.Application.Queries.Matching.List;
using FEA.URVP.Application.Queries.Matching.ListAssignmentCandidates;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Matching;

/// <summary>Student–project matching. Admin only.</summary>
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
        [FromQuery] bool includeManual,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        var runs = await _mediator.Send(
            new ListMatchingRunsQuery(semesterId, includeManual),
            cancellationToken);
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

    /// <summary>
    /// Students an admin can assign to a project. Recommended results share the
    /// project's research areas or are named in its qualifications.
    /// </summary>
    [HttpGet("projects/{projectId:guid}/assignment-candidates")]
    public async Task<IActionResult> ListAssignmentCandidates(
        Guid projectId,
        [FromQuery] string? search = null,
        [FromQuery] bool recommendedOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 8,
        CancellationToken cancellationToken = default)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, totalCount) = await _mediator.Send(
            new ListAssignmentCandidatesQuery(
                projectId,
                search,
                recommendedOnly,
                pageNumber,
                pageSize),
            cancellationToken);

        return PaginatedResponse(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>Assign a student to a project immediately. Occupies a seat.</summary>
    [HttpPost("assignments")]
    public async Task<IActionResult> Assign(
        [FromBody] AssignStudentToProjectCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return UnauthorizedResponse();

        command.CurrentUserId = userId;
        var placement = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(placement, "Student assigned");
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
