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
        CancellationToken cancellationToken = default);

    void Add(Project project);

    void Remove(Project project);
}
