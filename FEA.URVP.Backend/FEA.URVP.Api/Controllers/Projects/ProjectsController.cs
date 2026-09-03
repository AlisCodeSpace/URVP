using FEA.URVP.Api.Contracts;
using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Projects.Create;
using FEA.URVP.Application.Commands.Projects.Delete;
using FEA.URVP.Application.Commands.Projects.Update;
using FEA.URVP.Application.Queries.ProjectRankings.ListByProject;
using FEA.URVP.Application.Queries.Projects.GetAdminDetail;
using FEA.URVP.Application.Queries.Projects.ListParticipants;
using FEA.URVP.Application.Queries.Projects.GetById;
using FEA.URVP.Application.Queries.Projects.List;
using FEA.URVP.Application.Queries.Projects.ListAdmin;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Projects;

[ApiController]
[Route("api/projects")]
[Authorize]
public sealed class ProjectsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List projects (catalog). Pass mine=true for the caller's projects.</summary>
    /// <remarks>
    /// Authenticated: <see cref="ProjectDto"/> carries the posting faculty member's email address,
    /// which an anonymous catalog would let anyone harvest in bulk. Every frontend caller already
    /// sits behind a signed-in route, so this matches what the UI presents.
    /// </remarks>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool mine = false,
        [FromQuery] ProjectStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        Guid? createdBy = null;
        if (mine)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return UnauthorizedResponse();
            }

            createdBy = userId;
        }

        var (items, totalCount) = await _mediator.Send(
            new ListProjectsQuery(createdBy, status, pageNumber, pageSize),
            cancellationToken);

        return PaginatedResponse(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>List all projects with student ranking counts. Admin only.</summary>
    [HttpGet("admin")]
    public async Task<IActionResult> ListAdmin(
        [FromQuery] string? search = null,
        [FromQuery] ProjectStatus? status = null,
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
            new ListAdminProjectsQuery(search, status, pageNumber, pageSize),
            cancellationToken);

        return PaginatedResponse(items, pageNumber, pageSize, totalCount);
    }

    /// <summary>Project details plus students who ranked it. Admin only.</summary>
    [HttpGet("admin/{id:guid}")]
    public async Task<IActionResult> GetAdminDetail(Guid id, CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        var detail = await _mediator.Send(new GetAdminProjectDetailQuery(id), cancellationToken);
        return SuccessResponse(detail);
    }

    /// <summary>Students who ranked this project. Project owner or admin only.</summary>
    [HttpGet("{id:guid}/rankings")]
    public async Task<IActionResult> ListRankings(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var rankings = await _mediator.Send(
            new ListProjectRankingsQuery(id, userId, UserHasRole(nameof(UserRole.Admin))),
            cancellationToken);
        return SuccessResponse(rankings);
    }

    /// <summary>Students confirmed onto this project after matching. Project owner or admin only.</summary>
    [HttpGet("{id:guid}/participants")]
    public async Task<IActionResult> ListParticipants(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var participants = await _mediator.Send(
            new ListProjectParticipantsQuery(id, userId, UserHasRole(nameof(UserRole.Admin))),
            cancellationToken);
        return SuccessResponse(participants);
    }

    /// <remarks>Authenticated for the same reason as <see cref="List"/>.</remarks>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var project = await _mediator.Send(new GetProjectByIdQuery(id), cancellationToken);
        return SuccessResponse(project);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        command.CurrentUserId = userId;
        var project = await _mediator.Send(command, cancellationToken);

        var response = ApiResponse<object>.SuccessResult(project, "Project created");
        response.TraceId = HttpContext.TraceIdentifier;
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        command.ProjectId = id;
        command.CurrentUserId = userId;
        command.IsAdmin = UserHasRole(nameof(UserRole.Admin));

        var project = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(project, "Project updated");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        await _mediator.Send(
            new DeleteProjectCommand(id, userId, UserHasRole(nameof(UserRole.Admin))),
            cancellationToken);

        return SuccessResponse<object?>(null, "Project deleted");
    }
}
