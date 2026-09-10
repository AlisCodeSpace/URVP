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

    public async Task<IReadOnlyList<FileStorage>> ListActiveByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default) =>
        await _db.FileStorage
            .Where(f => f.EntityType == entityType && f.EntityId == entityId && !f.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FileStorage>> ListByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await _db.FileStorage
            .Where(f => ids.Contains(f.Id) && !f.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public void Add(FileStorage file) => _db.FileStorage.Add(file);
}
