using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Features.Projects.DTOs;

public sealed class ProjectResearchAssistantDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = null!;

    /// <summary>Account name, or null until the person has signed in.</summary>
    public string? Name { get; init; }

    public bool HasSignedIn { get; init; }
    public DateTime AddedAt { get; init; }
}

/// <summary>A project the signed-in person is listed on as a research assistant.</summary>
public sealed class AssistedProjectDto
{
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = null!;
    public string SemesterName { get; init; } = null!;
    public ProjectStatus Status { get; init; }
    public string FacultyName { get; init; } = null!;
    public string FacultyEmail { get; init; } = null!;
    public DateTime AddedAt { get; init; }
}
