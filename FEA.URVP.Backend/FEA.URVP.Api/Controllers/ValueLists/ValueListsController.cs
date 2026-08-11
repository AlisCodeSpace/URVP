using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.ValueLists.Create;
using FEA.URVP.Application.Commands.ValueLists.Delete;
using FEA.URVP.Application.Commands.ValueLists.Update;
using FEA.URVP.Application.Queries.ValueLists.List;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.ValueLists;

[ApiController]
[Route("api/value-lists/{kind}")]
[Authorize]
public sealed class ValueListsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ValueListsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        string kind,
        [FromQuery] string? search = null,
        [FromQuery] bool activeOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryParseKind(kind, out var listKind))
        {
            return ErrorResponse<object>("Unknown value list.", [$"Kind '{kind}' is not supported."]);
        }

        // Admin sees all (including inactive); others get active-only catalogs.
        var onlyActive = activeOnly || !UserHasRole(nameof(UserRole.Admin));

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var (items, totalCount) = await _mediator.Send(
            new ListValueListItemsQuery(listKind, search, onlyActive, pageNumber, pageSize),
            cancellationToken);

        return PaginatedResponse(items, pageNumber, pageSize, totalCount);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        string kind,
        [FromBody] CreateValueListItemCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        if (!TryParseKind(kind, out var listKind))
        {
            return ErrorResponse<object>("Unknown value list.", [$"Kind '{kind}' is not supported."]);
        }

        command.Kind = listKind;
        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "Value created");
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        string kind,
        Guid id,
        [FromBody] UpdateValueListItemCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        if (!TryParseKind(kind, out var listKind))
        {
            return ErrorResponse<object>("Unknown value list.", [$"Kind '{kind}' is not supported."]);
        }

        command.Id = id;
        command.Kind = listKind;
        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "Value updated");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        string kind,
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        if (!TryParseKind(kind, out var listKind))
        {
            return ErrorResponse<object>("Unknown value list.", [$"Kind '{kind}' is not supported."]);
        }

        await _mediator.Send(new DeleteValueListItemCommand(id, listKind), cancellationToken);
        return SuccessResponse<object?>(null, "Value deleted");
    }

    private static bool TryParseKind(string kind, out ValueListKind listKind)
    {
        switch (kind.Trim().ToLowerInvariant())
        {
            case "research-interests":
                listKind = ValueListKind.ResearchInterest;
                return true;
            case "research-areas":
                listKind = ValueListKind.ResearchArea;
                return true;
            default:
                listKind = default;
                return false;
        }
    }
}
