using FEA.URVP.Domain.Entities.Projects;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IProjectResearchAssistantRepository
{
    Task<IReadOnlyList<ProjectResearchAssistant>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    /// <summary>Assistant entries for this email, with their project and cycle loaded.</summary>
    Task<IReadOnlyList<ProjectResearchAssistant>> ListByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Names of people who have signed in, keyed by lowercase email.
    /// Emails with no account are absent from the result.
    /// </summary>
    Task<IReadOnlyDictionary<string, string>> NamesForEmailsAsync(
        IReadOnlyCollection<string> emails,
        CancellationToken cancellationToken = default);

    Task<ProjectResearchAssistant?> FindTrackedAsync(
        Guid projectId,
        Guid assistantId,
        CancellationToken cancellationToken = default);

    void Add(ProjectResearchAssistant assistant);

    void Remove(ProjectResearchAssistant assistant);
}
