using FEA.URVP.Application.Features.Projects.DTOs;
using FEA.URVP.Domain.Entities.Projects;

namespace FEA.URVP.Application.Features.Projects.Mappings;

public static class ProjectResearchAssistantMappings
{
    public static ProjectResearchAssistantDto ToDto(
        this ProjectResearchAssistant assistant,
        IReadOnlyDictionary<string, string> namesByEmail)
    {
        var hasName = namesByEmail.TryGetValue(assistant.Email, out var name);
        return new ProjectResearchAssistantDto
        {
            Id = assistant.Id,
            Email = assistant.Email,
            Name = hasName ? name : null,
            HasSignedIn = hasName,
            AddedAt = assistant.CreatedAt,
        };
    }

    public static AssistedProjectDto ToAssistedDto(this ProjectResearchAssistant assistant) =>
        new()
        {
            ProjectId = assistant.ProjectId,
            Title = assistant.Project.Title,
            SemesterName = assistant.Project.Semester?.Name ?? string.Empty,
            Status = assistant.Project.Status,
            FacultyName = assistant.Project.FacultyNameSnapshot,
            FacultyEmail = assistant.Project.EmailSnapshot,
            AddedAt = assistant.CreatedAt,
        };
}
