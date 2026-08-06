using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Files;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class FileStorageRepository : IFileStorageRepository
{
    private readonly AppDbContext _db;

    public FileStorageRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<FileStorage?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.FileStorage.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted, cancellationToken);

    public Task<FileStorage?> FindActiveByEntityAsync(
        string entityType,
        Guid entityId,
        string fileCategory,
        CancellationToken cancellationToken = default) =>
        _db.FileStorage.FirstOrDefaultAsync(
            f => f.EntityType == entityType
                && f.EntityId == entityId
                && f.FileCategory == fileCategory
                && !f.IsDeleted,
            cancellationToken);

    public void Add(FileStorage file) => _db.FileStorage.Add(file);
}
