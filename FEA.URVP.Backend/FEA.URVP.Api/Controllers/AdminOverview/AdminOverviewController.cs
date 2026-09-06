using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Queries.AdminOverview;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.AdminOverview;

[ApiController]
[Route("api/admin/overview")]
[Authorize]
public sealed class AdminOverviewController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AdminOverviewController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Aggregated dashboard metrics for the admin Overview page.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        var overview = await _mediator.Send(new GetAdminOverviewQuery(), cancellationToken);
        return SuccessResponse(overview);
    }
}
