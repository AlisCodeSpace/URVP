using System.Security.Cryptography;
using FEA.URVP.Application.Abstractions.Files;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Files;
using FEA.URVP.Application.Files;
using FEA.URVP.Application.Options;
using FEA.URVP.Application.StudentProfiles;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.Files;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Application.Commands.Files.Upload;

public sealed class UploadFileCommandHandler
    : BaseCommandHandler<UploadFileCommand, FileMetadataDto>
{
    private readonly IFileStorageRepository _files;
    private readonly IUserRepository _users;
    private readonly IWorkshopRepository _workshops;
    private readonly IMimeTypeValidator _mimeTypeValidator;
    private readonly FileStorageOptions _fileStorage;

    public UploadFileCommandHandler(
        ILogger<UploadFileCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IFileStorageRepository files,
        IUserRepository users,
        IWorkshopRepository workshops,
        IMimeTypeValidator mimeTypeValidator,
        IOptions<FileStorageOptions> fileStorage)
        : base(logger, unitOfWork)
    {
        _files = files;
        _users = users;
        _workshops = workshops;
        _mimeTypeValidator = mimeTypeValidator;
        _fileStorage = fileStorage.Value;
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

        var file = request.File;
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("File is required.");
        }

        // Detect once and reuse for type checks, logging, and persistence.
        // Do not call IsPdfAsync / IsImageAsync / DetectMimeTypeAsync again.
        var detectedMime = await _mimeTypeValidator.DetectMimeTypeAsync(file);
        var fileName = Path.GetFileName(file.FileName.Trim());
        var isImage = FileStorageCatalog.IsImageCategory(request.FileCategory);

        FileUploadRules.EnsureAccepted(
            [(detectedMime, file)],
            isImage,
            _fileStorage);

        Logger.LogInformation(
            "Accepted upload {FileName} ({FileSize} bytes) as {MimeType} for {Category}",
            fileName,
            file.Length,
            detectedMime,
            request.FileCategory);

        await using var contentStream = file.OpenReadStream();
        if (contentStream.CanSeek)
        {
            contentStream.Seek(0, SeekOrigin.Begin);
        }

        using var memory = new MemoryStream();
        await contentStream.CopyToAsync(memory, cancellationToken);
        var content = memory.ToArray();

        var existing = await _files.FindActiveByEntityAsync(
            request.EntityType,
            request.EntityId,
            request.FileCategory,
            cancellationToken);

        if (existing is not null)
        {
            existing.IsDeleted = true;
        }

        var hash = SHA256.HashData(content);
        var stored = new FileStorage
        {
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            FileCategory = request.FileCategory,
            FileName = fileName,
            MimeType = detectedMime ?? "application/octet-stream",
            FileSize = content.LongLength,
            ContentHash = hash,
            Content = content,
            UploadedBy = user.Id,
            UploadedAt = DateTime.UtcNow,
            IsDeleted = false,
        };

        _files.Add(stored);

        if (request.EntityType == FileStorageCatalog.EntityWorkshop)
        {
            var workshop = await _workshops.FindByIdAsync(request.EntityId, cancellationToken)
                ?? throw new KeyNotFoundException($"Workshop {request.EntityId} was not found.");
            workshop.PosterFileId = stored.Id;
            workshop.UpdatedAt = DateTime.UtcNow;
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        Logger.LogInformation(
            "Stored file {FileId} ({Category}) for {EntityType}/{EntityId}",
            stored.Id,
            stored.FileCategory,
            stored.EntityType,
            stored.EntityId);

        return new FileMetadataDto
        {
            Id = stored.Id,
            EntityType = stored.EntityType,
            EntityId = stored.EntityId,
            FileCategory = stored.FileCategory,
            FileName = stored.FileName,
            MimeType = stored.MimeType,
            FileSize = stored.FileSize,
            UploadedAt = stored.UploadedAt,
        };
    }
}
