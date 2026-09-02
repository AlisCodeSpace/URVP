using FEA.URVP.Domain.Entities.Semesters;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface ISemesterRepository
{
    Task<Semester?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Semester?> FindActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Semester>> ListAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends the running/upcoming cycle on every semester except
    /// <paramref name="exceptId"/> so only one cycle can be current.
    /// </summary>
    Task RelinquishAllExceptAsync(
        Guid exceptId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<Semester?> FindOverlappingCycleAsync(
        Guid? excludeId,
        DateTime start,
        DateTime? end,
        CancellationToken cancellationToken = default);

    void Add(Semester semester);

    void Remove(Semester semester);
}
