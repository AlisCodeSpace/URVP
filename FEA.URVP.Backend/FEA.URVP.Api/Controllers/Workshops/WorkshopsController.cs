using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Workshops.Create;
using FEA.URVP.Application.Commands.Workshops.Delete;
using FEA.URVP.Application.Commands.Workshops.Update;
using FEA.URVP.Application.Queries.Workshops.GetById;
using FEA.URVP.Application.Queries.Workshops.List;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Workshops;

[ApiController]
[Route("api/workshops")]
public sealed class WorkshopsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public WorkshopsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var (items, totalCount) = await _mediator.Send(
            new ListWorkshopsQuery(search, pageNumber, pageSize),
            cancellationToken);

        return PaginatedResponse(items, pageNumber, pageSize, totalCount);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _mediator.Send(new GetWorkshopByIdQuery(id), cancellationToken);
        return SuccessResponse(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateWorkshopCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "Workshop created");
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateWorkshopCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        command.Id = id;
        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "Workshop updated");
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        await _mediator.Send(new DeleteWorkshopCommand(id), cancellationToken);
        return SuccessResponse<object?>(null, "Workshop deleted");
    }
}
