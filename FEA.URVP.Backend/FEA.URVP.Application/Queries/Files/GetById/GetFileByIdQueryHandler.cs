using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Files;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Queries.Files.GetById;

public sealed class GetFileByIdQueryHandler : IRequestHandler<GetFileByIdQuery, FileContentDto>
{
    private readonly IFileStorageRepository _files;
    private readonly IUserRepository _users;
    private readonly IProjectRankingRepository _rankings;

    public GetFileByIdQueryHandler(
        IFileStorageRepository files,
        IUserRepository users,
        IProjectRankingRepository rankings)
    {
        _files = files;
        _users = users;
        _rankings = rankings;
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
            await EnsureFacultyCanDownloadStudentFileAsync(request, file.EntityType, file.EntityId, cancellationToken);
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
        };

    private async Task EnsureFacultyCanDownloadStudentFileAsync(
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

        if (viewer.Role is not UserRole.Faculty)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this file.");
        }

        var ranked = await _rankings.StudentHasRankedFacultyProjectAsync(
            studentUserId,
            viewer.Id,
            cancellationToken);

        if (!ranked)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this file.");
        }
    }
}
