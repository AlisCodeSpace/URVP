using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Users.AssignRole;
using FEA.URVP.Application.Queries.Users.List;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Users;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List users with optional search, role filters, and sorting. Admin only.</summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search = null,
        [FromQuery] UserRole? role = null,
        [FromQuery] UserSortField sortBy = UserSortField.Name,
        [FromQuery] SortDirection sortDir = SortDirection.Asc,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, totalCount) = await _mediator.Send(
            new ListUsersQuery(search, role, sortBy, sortDir, pageNumber, pageSize),
            cancellationToken);

        return PaginatedResponse(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>Assign a role to a user account. Admin only.</summary>
    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> AssignRole(
        Guid id,
        [FromBody] AssignUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        command.UserId = id;
        command.CurrentUserId = userId;

        var user = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(user, "Role updated");
    }
}
