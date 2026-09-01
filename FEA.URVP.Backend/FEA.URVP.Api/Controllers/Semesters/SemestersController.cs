using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Semesters.Create;
using FEA.URVP.Application.Commands.Semesters.Delete;
using FEA.URVP.Application.Commands.Semesters.SetActive;
using FEA.URVP.Application.Commands.Semesters.SetApplicationWindow;
using FEA.URVP.Application.Commands.Semesters.Update;
using FEA.URVP.Application.Queries.Semesters.GetActive;
using FEA.URVP.Application.Queries.Semesters.GetById;
using FEA.URVP.Application.Queries.Semesters.List;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Semesters;

[ApiController]
[Route("api/semesters")]
public sealed class SemestersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public SemestersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Returns all semesters, most-recent first.</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var items = await _mediator.Send(new ListSemestersQuery(), cancellationToken);
        return SuccessResponse(items);
    }

    /// <summary>Returns the currently active semester, or null.</summary>
    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var item = await _mediator.Send(new GetActiveSemesterQuery(), cancellationToken);
        return SuccessResponse(item);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _mediator.Send(new GetSemesterByIdQuery(id), cancellationToken);
        return SuccessResponse(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSemesterCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "Semester created");
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSemesterCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        command.Id = id;
        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "Semester updated");
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        await _mediator.Send(new DeleteSemesterCommand { Id = id }, cancellationToken);
        return SuccessResponse<object?>(null, "Semester deleted");
    }

    /// <summary>
    /// Starts or ends the academic cycle for this semester.
    /// Starting a cycle automatically deactivates all other semesters.
    /// </summary>
    [Authorize]
    [HttpPost("{id:guid}/set-active")]
    public async Task<IActionResult> SetActive(
        Guid id,
        [FromBody] SetActiveRequest body,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        var item = await _mediator.Send(
            new SetSemesterActiveCommand { Id = id, IsActive = body.IsActive },
            cancellationToken);
        var msg = body.IsActive ? "Semester cycle started" : "Semester cycle ended";
        return SuccessResponse(item, msg);
    }

    /// <summary>
    /// Opens or closes the student application window.
    /// Pass null for ApplicationWindowStart to reset the window entirely.
    /// Pass null for ApplicationWindowEnd to leave the window open indefinitely.
    /// </summary>
    [Authorize]
    [HttpPost("{id:guid}/set-application-window")]
    public async Task<IActionResult> SetApplicationWindow(
        Guid id,
        [FromBody] SetApplicationWindowCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
            return ForbiddenResponse();

        command.Id = id;
        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "Application window updated");
    }
}

public sealed class SetActiveRequest
{
    public bool IsActive { get; init; }
}
