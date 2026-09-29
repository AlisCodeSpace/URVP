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

    void AddRange(IEnumerable<ProjectAlumni> alumni);
}
