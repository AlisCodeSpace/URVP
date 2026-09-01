using FEA.URVP.Domain.Entities.Semesters;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface ISemesterRepository
{
    Task<Semester?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Semester?> FindActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Semester>> ListAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Sets IsActive = false on every semester except <paramref name="exceptId"/>.</summary>
    Task DeactivateAllExceptAsync(Guid exceptId, CancellationToken cancellationToken = default);

    void Add(Semester semester);

    void Remove(Semester semester);
}
