using FEA.URVP.Domain.Entities.Files;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IFileStorageRepository
{
    Task<FileStorage?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<FileStorage?> FindActiveByEntityAsync(
        string entityType,
        Guid entityId,
        string fileCategory,
        CancellationToken cancellationToken = default);

    void Add(FileStorage file);
}
