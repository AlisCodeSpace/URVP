using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Application.Commands.Files.Upload;
using FEA.URVP.Application.Queries.Files.GetById;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FEA.URVP.Api.Controllers.Files;

[ApiController]
[Route("api/files")]
[Authorize]
public sealed class FilesController : ApiControllerBase
{
    /// <summary>
    /// Content types that may render in the browser. Anything else is forced to download, so a
    /// stored file can never be interpreted as an active document on this origin. SVG is absent
    /// deliberately: it is scriptable, and upload validation rejects it.
    /// </summary>
    private static readonly HashSet<string> InlineRenderableMimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/gif"
        };

    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Upload a file into SQL FileStorage (student PDFs or workshop posters).</summary>
    /// <remarks>
    /// The client-supplied <paramref name="entityType"/>, <paramref name="entityId"/> and
    /// <paramref name="fileCategory"/> are untrusted routing hints. Ownership and role
    /// authorization, magic-byte type detection and size limits are all enforced in
    /// <see cref="UploadFileCommandHandler"/> and its validator.
    /// </remarks>
    [HttpPost]
    [RequestSizeLimit(FileStorageCatalog.MaxTotalSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FileStorageCatalog.MaxTotalSizeBytes)]
    [EnableRateLimiting(RateLimitingConfiguration.UploadPolicy)]
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

        var metadata = await _mediator.Send(
            new UploadFileCommand
            {
                CurrentUserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                FileCategory = fileCategory,
                File = file,
            },
            cancellationToken);

        return SuccessResponse(metadata, "File uploaded");
    }

    /// <summary>
    /// Download a file stored in SQL.
    /// </summary>
    /// <remarks>
    /// Anonymous by design because workshop posters are public assets embedded in public pages.
    /// Every other file is authorized inside <see cref="GetFileByIdQueryHandler"/>, which
    /// requires the owner, an administrator, or a faculty member the student has ranked.
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [EnableRateLimiting(RateLimitingConfiguration.DownloadPolicy)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var file = await _mediator.Send(
            new GetFileByIdQuery(id, GetCurrentUserId(), UserHasRole(nameof(UserRole.Admin))),
            cancellationToken);

        // Only genuinely public files may be stored by shared caches; an authorization-gated file
        // must not survive in a proxy where the next requester would bypass the check.
        Response.Headers.CacheControl = file.IsPublic
            ? "public, max-age=86400"
            : "no-store, no-cache, must-revalidate";

        if (file.IsPublic && InlineRenderableMimeTypes.Contains(file.MimeType))
        {
            return File(file.Content, file.MimeType);
        }

        return File(file.Content, file.MimeType, file.FileName);
    }
}
