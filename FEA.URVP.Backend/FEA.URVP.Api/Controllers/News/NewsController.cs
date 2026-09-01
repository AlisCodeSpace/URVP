using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.News.Create;
using FEA.URVP.Application.Commands.News.Delete;
using FEA.URVP.Application.Commands.News.Update;
using FEA.URVP.Application.Queries.News.GetById;
using FEA.URVP.Application.Queries.News.GetBySlug;
using FEA.URVP.Application.Queries.News.List;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.News;

[ApiController]
[Route("api/news")]
public sealed class NewsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public NewsController(IMediator mediator)
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
            new ListNewsArticlesQuery(search, pageNumber, pageSize),
            cancellationToken);

        return PaginatedResponse(items, pageNumber, pageSize, totalCount);
    }

    [AllowAnonymous]
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var item = await _mediator.Send(new GetNewsArticleBySlugQuery(slug), cancellationToken);
        return SuccessResponse(item);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        var item = await _mediator.Send(new GetNewsArticleByIdQuery(id), cancellationToken);
        return SuccessResponse(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateNewsArticleCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "News article created");
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateNewsArticleCommand command,
        CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        command.Id = id;
        var item = await _mediator.Send(command, cancellationToken);
        return SuccessResponse(item, "News article updated");
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!UserHasRole(nameof(UserRole.Admin)))
        {
            return ForbiddenResponse();
        }

        await _mediator.Send(new DeleteNewsArticleCommand(id), cancellationToken);
        return SuccessResponse<object?>(null, "News article deleted");
    }
}
