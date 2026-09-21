using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Users.AssignRole;
using FEA.URVP.Application.Queries.Users.Export;
using FEA.URVP.Application.Queries.Users.List;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

    /// <summary>
    /// Download matching user accounts as PDF or Excel. Admin only.
    /// Honors the same search, role, and sort filters as the users list.
    /// </summary>
    [HttpGet("export")]
    [EnableRateLimiting(RateLimitingConfiguration.DownloadPolicy)]
    public async Task<IActionResult> Export(
        [FromQuery] string? format = "xlsx",
        [FromQuery] string? search = null,
        [FromQuery] UserRole? role = null,
        [FromQuery] UserSortField sortBy = UserSortField.Name,
        [FromQuery] SortDirection sortDir = SortDirection.Asc,
        CancellationToken cancellationToken = default)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        var file = await _mediator.Send(
            new ExportUsersQuery(format, search, role, sortBy, sortDir),
            cancellationToken);
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        return File(file.Content, file.MimeType, file.FileName);
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
