using FEA.URVP.Domain.Entities.Matching;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IMatchingRunRepository
{
    /// <summary>Tracked run with placements, project and student navigations loaded.</summary>
    Task<MatchingRun?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Detached run graph for API responses.</summary>
    Task<MatchingRun?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchingRun>> ListAsync(
        Guid? semesterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MatchingRun>> ListDraftsBySemesterAsync(
        Guid semesterId,
        CancellationToken cancellationToken = default);

    /// <summary>Placements that currently occupy a seat within the semester.</summary>
    Task<IReadOnlyList<Placement>> ListConfirmedPlacementsAsync(
        Guid semesterId,
        CancellationToken cancellationToken = default);

    Task<Placement?> FindPlacementByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirmed placements that belong to the project's current academic cycle.
    /// Placements from an earlier cycle do not occupy a seat.
    /// </summary>
    Task<int> CountConfirmedByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>Confirmed volunteers on the project's current academic cycle.</summary>
    Task<IReadOnlyList<Placement>> ListConfirmedByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Project ids the student occupies via a confirmed placement in the given cycle.</summary>
    Task<IReadOnlyList<Guid>> ListConfirmedProjectIdsByStudentAsync(
        Guid studentUserId,
        Guid semesterId,
        CancellationToken cancellationToken = default);

    /// <summary>Tracked per-semester run that stores admin-made assignments.</summary>
    Task<MatchingRun?> FindManualBySemesterAsync(
        Guid semesterId,
        CancellationToken cancellationToken = default);

    /// <summary>Confirmed placements for the given students (at most one each in normal use).</summary>
    Task<IReadOnlyList<Placement>> ListConfirmedByStudentIdsAsync(
        IReadOnlyCollection<Guid> studentUserIds,
        CancellationToken cancellationToken = default);

    /// <summary>Confirmed placements on the given projects, with the student loaded.</summary>
    Task<IReadOnlyList<Placement>> ListConfirmedByProjectIdsAsync(
        IReadOnlyCollection<Guid> projectIds,
        CancellationToken cancellationToken = default);

    void Add(MatchingRun run);

    /// <summary>
    /// Tracks a new placement for insert. Placement ids are client-generated
    /// Guids, so adding the entity only to <see cref="MatchingRun.Placements"/>
    /// marks it modified when the run is already tracked.
    /// </summary>
    void AddPlacement(Placement placement);
}
