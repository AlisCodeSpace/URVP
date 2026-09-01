using System.Security.Cryptography;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Files;
using FEA.URVP.Application.StudentProfiles;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.Files;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Commands.Files.Upload;

public sealed class UploadFileCommandHandler
    : BaseCommandHandler<UploadFileCommand, FileMetadataDto>
{
    private readonly IFileStorageRepository _files;
    private readonly IUserRepository _users;
    private readonly IWorkshopRepository _workshops;

    public UploadFileCommandHandler(
        ILogger<UploadFileCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IFileStorageRepository files,
        IUserRepository users,
        IWorkshopRepository workshops)
        : base(logger, unitOfWork)
    {
        _files = files;
        _users = users;
        _workshops = workshops;
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
        else if (request.EntityType == FileStorageCatalog.EntityWorkshop)
        {
            if (user.Role is not UserRole.Admin)
            {
                throw new UnauthorizedAccessException("Only administrators can upload workshop posters.");
            }

            if (request.FileCategory != FileStorageCatalog.CategoryPoster)
            {
                throw new ArgumentException("Workshop files must use the Poster category.");
            }

            _ = await _workshops.FindByIdAsync(request.EntityId, cancellationToken)
                ?? throw new KeyNotFoundException($"Workshop {request.EntityId} was not found.");
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
            ? (FileStorageCatalog.IsImageCategory(request.FileCategory) ? "image/jpeg" : "application/pdf")
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

        if (request.EntityType == FileStorageCatalog.EntityWorkshop)
        {
            var workshop = await _workshops.FindByIdAsync(request.EntityId, cancellationToken)
                ?? throw new KeyNotFoundException($"Workshop {request.EntityId} was not found.");
            workshop.PosterFileId = file.Id;
            workshop.UpdatedAt = DateTime.UtcNow;
        }

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
