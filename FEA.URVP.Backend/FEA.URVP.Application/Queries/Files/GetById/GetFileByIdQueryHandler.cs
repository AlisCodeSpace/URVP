using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Files;
using FEA.URVP.Domain.Catalog;
using MediatR;

namespace FEA.URVP.Application.Queries.Files.GetById;

public sealed class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQuery, FileContentDto>
{
    private readonly IFileStorageRepository _files;

    public GetFileByIdQueryHandler(IFileStorageRepository files)
    {
        _files = files;
    }

    public async Task<FileContentDto> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        var file = await _files.FindByIdAsync(request.FileId, cancellationToken)
            ?? throw new KeyNotFoundException($"File {request.FileId} was not found.");

        if (!request.IsAdmin
            && file.EntityType == FileStorageCatalog.EntityStudentProfile
            && file.EntityId != request.CurrentUserId
            && file.UploadedBy != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this file.");
        }

        return new FileContentDto
        {
            Id = file.Id,
            FileName = file.FileName,
            MimeType = file.MimeType,
            Content = file.Content,
            ContentHash = file.ContentHash,
        };
    }
}
