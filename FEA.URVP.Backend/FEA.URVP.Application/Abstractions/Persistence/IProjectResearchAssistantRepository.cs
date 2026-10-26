using FEA.URVP.Domain.Entities.Projects;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IProjectResearchAssistantRepository
{
    Task<IReadOnlyList<ProjectResearchAssistant>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<ProjectResearchAssistant?> FindTrackedAsync(
        Guid projectId,
        Guid assistantId,
        CancellationToken cancellationToken = default);

    void Add(ProjectResearchAssistant assistant);

    void Remove(ProjectResearchAssistant assistant);
}
