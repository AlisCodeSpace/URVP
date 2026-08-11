using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Divisions.Create;
using FEA.URVP.Application.Commands.Divisions.Delete;
using FEA.URVP.Application.Commands.Divisions.Update;
using FEA.URVP.Application.Queries.Divisions.List;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Divisions;

[ApiController]
[Route("api/divisions")]
[Authorize]
public sealed class DivisionsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DivisionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search = null,
        [FromQuery] bool activeOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Admin sees all (including inactive); others get active-only catalogs.
        var onlyActive = activeOnly || !UserHasRole(nameof(UserRole.Admin));

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var (items, totalCount) = await _mediator.Send(
            new ListDivisionsQuery(search, onlyActive, pageNumber, pageSize),
            cancellationToken);

        return PaginatedResponse(items, pageNumber, pageSize, totalCount);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDivisionCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "Division created");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDivisionCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        command.Id = id;
        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "Division updated");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        await _mediator.Send(new DeleteDivisionCommand(id), cancellationToken);
        return SuccessResponse<object?>(null, "Division deleted");
    }
}
