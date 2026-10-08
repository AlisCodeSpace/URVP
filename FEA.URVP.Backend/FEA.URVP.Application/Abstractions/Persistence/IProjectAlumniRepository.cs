using FEA.URVP.Domain.Entities.Projects;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IProjectAlumniRepository
{
    Task<bool> AnyForCycleAsync(
        Guid projectId,
        Guid semesterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectAlumni>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// True when the student was archived as a confirmed participant on a project
    /// posted by the faculty member.
    /// </summary>
    Task<bool> StudentWasConfirmedOnFacultyProjectAsync(
        Guid studentUserId,
        Guid facultyUserId,
        CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<ProjectAlumni> alumni);
}
