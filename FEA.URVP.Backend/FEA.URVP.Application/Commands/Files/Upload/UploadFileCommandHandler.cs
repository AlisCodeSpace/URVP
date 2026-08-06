using System.Security.Cryptography;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Files;
using FEA.URVP.Application.StudentProfiles;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.Files;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Files.Upload;

public sealed class UploadFileCommandHandler
    : BaseCommandHandler<UploadFileCommand, FileMetadataDto>
{
    private readonly IFileStorageRepository _files;
    private readonly IUserRepository _users;

    public UploadFileCommandHandler(
        ILogger<UploadFileCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IFileStorageRepository files,
        IUserRepository users)
        : base(logger, unitOfWork)
    {
        _files = files;
        _users = users;
    }

    protected override async Task<FileMetadataDto> HandleInternal(
        UploadFileCommand request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var user = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        if (request.EntityType == FileStorageCatalog.EntityStudentProfile)
        {
            StudentProfileAccess.EnsureCanManage(user.Role, user.Email);

            if (request.EntityId != user.Id)
            {
                throw new UnauthorizedAccessException("You can only upload files for your own profile.");
            }
        }
        else
        {
            throw new ArgumentException($"Entity type '{request.EntityType}' is not supported.");
        }

        var existing = await _files.FindActiveByEntityAsync(
            request.EntityType,
            request.EntityId,
            request.FileCategory,
            cancellationToken);

        if (existing is not null)
        {
            existing.IsDeleted = true;
        }

        var hash = SHA256.HashData(request.Content);
        var mimeType = string.IsNullOrWhiteSpace(request.ContentType)
            ? "application/pdf"
            : request.ContentType.Trim();

        var file = new FileStorage
        {
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            FileCategory = request.FileCategory,
            FileName = Path.GetFileName(request.FileName.Trim()),
            MimeType = mimeType,
            FileSize = request.Content.LongLength,
            ContentHash = hash,
            Content = request.Content,
            UploadedBy = user.Id,
            UploadedAt = DateTime.UtcNow,
            IsDeleted = false,
        };

        _files.Add(file);
        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Stored file {FileId} ({Category}) for {EntityType}/{EntityId}",
            file.Id,
            file.FileCategory,
            file.EntityType,
            file.EntityId);

        return new FileMetadataDto
        {
            Id = file.Id,
            EntityType = file.EntityType,
            EntityId = file.EntityId,
            FileCategory = file.FileCategory,
            FileName = file.FileName,
            MimeType = file.MimeType,
            FileSize = file.FileSize,
            UploadedAt = file.UploadedAt,
        };
    }
}
