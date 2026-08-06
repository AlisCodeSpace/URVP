using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Files.Upload;
using FEA.URVP.Application.Queries.Files.GetById;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Files;

[ApiController]
[Route("api/files")]
[Authorize]
public sealed class FilesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Upload a PDF into SQL FileStorage (StudentProfile documents).</summary>
    [HttpPost]
    [RequestSizeLimit(FileStorageCatalog.MaxDocumentBytes)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] string entityType,
        [FromForm] Guid entityId,
        [FromForm] string fileCategory,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        if (file is null || file.Length == 0)
        {
            return ErrorResponse<object>("File is required.");
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var metadata = await _mediator.Send(
            new UploadFileCommand
            {
                CurrentUserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                FileCategory = fileCategory,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Content = memory.ToArray(),
            },
            cancellationToken);

        return SuccessResponse(metadata, "File uploaded");
    }

    /// <summary>Download a file stored in SQL.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            return UnauthorizedResponse();
        }

        var file = await _mediator.Send(
            new GetFileByIdQuery(id, userId, UserHasRole(nameof(UserRole.Admin))),
            cancellationToken);

        return File(file.Content, file.MimeType, file.FileName);
    }
}
