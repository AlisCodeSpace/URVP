using FEA.URVP.Domain.Entities.ProjectRankings;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IProjectRankingRepository
{
    Task<IReadOnlyList<ProjectRanking>> ListByStudentAsync(
        Guid studentUserId,
        CancellationToken cancellationToken = default);

    Task<ProjectRanking?> FindByStudentAndProjectAsync(
        Guid studentUserId,
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<ProjectRanking?> FindByStudentAndRankAsync(
        Guid studentUserId,
        byte rank,
        CancellationToken cancellationToken = default);

    /// <summary>For matching / admin: all rankings for a project, ordered by rank then date.</summary>
    Task<IReadOnlyList<ProjectRanking>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// True when the student has ranked at least one project posted by the faculty member.
    /// </summary>
    Task<bool> StudentHasRankedFacultyProjectAsync(
        Guid studentUserId,
        Guid facultyUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectRanking>> ListByProjectIdsAsync(
        IReadOnlyCollection<Guid> projectIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, int>> CountByProjectIdsAsync(
        IReadOnlyCollection<Guid> projectIds,
        CancellationToken cancellationToken = default);

    void Add(ProjectRanking ranking);

    void Remove(ProjectRanking ranking);
}
