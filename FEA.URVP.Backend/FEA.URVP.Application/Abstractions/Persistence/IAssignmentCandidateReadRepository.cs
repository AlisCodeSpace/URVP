using FEA.URVP.Application.Queries.Matching.ListAssignmentCandidates;

namespace FEA.URVP.Application.Abstractions.Persistence;

/// <summary>
/// Reads assignment candidates without loading full user, profile, or placement graphs.
/// Only students with a saved profile are included. Students already confirmed
/// on any current-cycle project are excluded in the database.
/// </summary>
public interface IAssignmentCandidateReadRepository
{
    /// <summary>Every eligible student, for scoring the recommended list.</summary>
    Task<IReadOnlyList<AssignmentStudentSource>> ListEligibleAsync(
        Guid projectId,
        string? search,
        CancellationToken cancellationToken = default);

    /// <summary>One page of eligible students, ordered by name.</summary>
    Task<(IReadOnlyList<AssignmentStudentSource> Items, int TotalCount)> PageEligibleAsync(
        Guid projectId,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Confirmed placement on another project, for the students on this page.</summary>
    Task<IReadOnlyDictionary<Guid, ConfirmedPlacementLabel>> LabelsForStudentsAsync(
        IReadOnlyCollection<Guid> studentUserIds,
        CancellationToken cancellationToken = default);
}
