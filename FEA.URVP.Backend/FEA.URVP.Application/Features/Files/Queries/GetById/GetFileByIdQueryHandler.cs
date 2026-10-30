using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Files;
using FEA.URVP.Application.StudentProfiles;
using FEA.URVP.Domain.Catalog;
using MediatR;

namespace FEA.URVP.Application.Queries.Files.GetById;

public sealed class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQuery, FileContentDto>
{
    private readonly IFileStorageRepository _files;
    private readonly IUserRepository _users;
    private readonly StudentProfileViewAccess _viewAccess;

    public GetFileByIdQueryHandler(
        IFileStorageRepository files,
        IUserRepository users,
        StudentProfileViewAccess viewAccess)
    {
        _files = files;
        _users = users;
        _viewAccess = viewAccess;
    }

    public async Task<FileContentDto> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        var file = await _files.FindByIdAsync(request.FileId, cancellationToken)
            ?? throw new KeyNotFoundException($"File {request.FileId} was not found.");

        if (FileStorageCatalog.IsPublicFile(file.EntityType, file.FileCategory))
        {
            return ToContent(file);
        }

        var isOwner =
            file.EntityId == request.CurrentUserId
            || file.UploadedBy == request.CurrentUserId;

        if (!request.IsAdmin && !isOwner)
        {
            await EnsureCanDownloadStudentFileAsync(request, file.EntityType, file.EntityId, cancellationToken);
        }

        return ToContent(file);
    }

    private static FileContentDto ToContent(Domain.Entities.Files.FileStorage file) =>
        new()
        {
            Id = file.Id,
            FileName = file.FileName,
            MimeType = file.MimeType,
            Content = file.Content,
            ContentHash = file.ContentHash,
            IsPublic = FileStorageCatalog.IsPublicFile(file.EntityType, file.FileCategory),
        };

    private async Task EnsureCanDownloadStudentFileAsync(
        GetFileByIdQuery request,
        string entityType,
        Guid studentUserId,
        CancellationToken cancellationToken)
    {
        if (entityType != FileStorageCatalog.EntityStudentProfile)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this file.");
        }

        var viewer = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (!await _viewAccess.CanViewAsync(viewer, studentUserId, cancellationToken))
        {
            throw new UnauthorizedAccessException("You do not have permission to access this file.");
        }
    }
}
