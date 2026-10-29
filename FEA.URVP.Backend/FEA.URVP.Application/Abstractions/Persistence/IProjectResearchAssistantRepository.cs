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

    /// <summary>True when this email is listed on the project.</summary>
    Task<bool> IsListedOnProjectAsync(
        Guid projectId,
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// True when the student ranked, is assigned to, or was confirmed on a project this email assists.
    /// </summary>
    Task<bool> CanViewStudentAsync(
        string email,
        Guid studentUserId,
        CancellationToken cancellationToken = default);

    Task<ProjectResearchAssistant?> FindTrackedAsync(
        Guid projectId,
        Guid assistantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts the assistant. Returns false when that email is already listed on the project,
    /// including when two adds arrive together and the unique index rejects the second.
    /// </summary>
    Task<bool> TryAddAsync(
        ProjectResearchAssistant assistant,
        CancellationToken cancellationToken = default);

    void Remove(ProjectResearchAssistant assistant);
}
