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

    /// <summary>Confirmed placements per project across all semesters (drives seat accounting).</summary>
    Task<int> CountConfirmedByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);

    void Add(MatchingRun run);
}
