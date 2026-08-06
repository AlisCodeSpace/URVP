using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.ProjectRankings.Remove;
using FEA.URVP.Application.Commands.ProjectRankings.Upsert;
using FEA.URVP.Application.Queries.ProjectRankings.GetMine;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.ProjectRankings;

[ApiController]
[Route("api/project-rankings")]
[Authorize]
public sealed class ProjectRankingsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ProjectRankingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List the signed-in student's project rankings (ordered by rank 1–3).</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var rankings = await _mediator.Send(new GetMyProjectRankingsQuery(userId), cancellationToken);
        return SuccessResponse(rankings);
    }

    /// <summary>
    /// Assign or update a rank (1–3) for a project. Replaces any project already in that rank slot.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertProjectRankingCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        command.CurrentUserId = userId;
        var ranking = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(ranking, "Ranking saved");
    }

    /// <summary>Remove a ranking for a project.</summary>
    [HttpDelete("{projectId:guid}")]
    public async Task<IActionResult> Remove(Guid projectId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var command = new RemoveProjectRankingCommand(projectId) { CurrentUserId = userId };
        await _mediator.Send(command, cancellationToken);
        return SuccessResponse<object?>(null, "Ranking removed");
    }
}
