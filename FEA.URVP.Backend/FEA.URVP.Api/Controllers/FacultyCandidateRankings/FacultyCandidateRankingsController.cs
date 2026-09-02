using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.FacultyCandidateRankings.Remove;
using FEA.URVP.Application.Commands.FacultyCandidateRankings.Upsert;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.FacultyCandidateRankings;

[ApiController]
[Route("api/faculty-candidate-rankings")]
[Authorize]
public sealed class FacultyCandidateRankingsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public FacultyCandidateRankingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Assign or update a faculty rank for a student who applied to the project.</summary>
    [HttpPut]
    public async Task<IActionResult> Upsert(
        [FromBody] UpsertFacultyCandidateRankingCommand command,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        command.CurrentUserId = userId;
        command.IsAdmin = UserHasRole(nameof(UserRole.Admin));
        var ranking = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(ranking, "Candidate ranking saved");
    }

    /// <summary>Remove a faculty ranking for a student on a project.</summary>
    [HttpDelete("{projectId:guid}/{studentUserId:guid}")]
    public async Task<IActionResult> Remove(
        Guid projectId,
        Guid studentUserId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var command = new RemoveFacultyCandidateRankingCommand(projectId, studentUserId)
        {
            CurrentUserId = userId,
            IsAdmin = UserHasRole(nameof(UserRole.Admin)),
        };
        await _mediator.Send(command, cancellationToken);
        return SuccessResponse<object?>(null, "Candidate ranking removed");
    }
}
