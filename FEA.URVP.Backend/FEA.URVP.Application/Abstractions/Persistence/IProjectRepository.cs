using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IProjectRepository
{
    Task<Project?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Project> Items, int TotalCount)> ListAsync(
        Guid? createdByUserId,
        ProjectStatus? status,
        int pageNumber,
        int pageSize,
        Guid? semesterId,
        bool excludeInactive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracked projects whose academic cycle has already ended and that are
    /// not yet marked inactive.
    /// </summary>
    Task<IReadOnlyList<Project>> ListTrackedOnEndedCyclesAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Project> Items, int TotalCount)> ListForAdminAsync(
        string? search,
        ProjectStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Every project matching the admin list filters, without paging.</summary>
    Task<IReadOnlyList<Project>> ListAllForAdminAsync(
        string? search,
        ProjectStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>Tracked projects in the given status (used by matching to adjust seat counts).</summary>
    Task<IReadOnlyList<Project>> ListByStatusAsync(
        ProjectStatus status,
        CancellationToken cancellationToken = default);

    void Add(Project project);

    void Remove(Project project);
}
