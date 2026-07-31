using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.DTOs.Projects;

public sealed class ProjectDto
{
    public Guid Id { get; init; }
    public Guid CreatedByUserId { get; init; }
    public string Title { get; init; } = null!;
    public IReadOnlyList<string> ResearchAreas { get; init; } = [];
    public IrbStage IrbStage { get; init; }
    public string IrbStageLabel { get; init; } = null!;
    public string BriefDescription { get; init; } = null!;
    public IReadOnlyList<string> ActivityTypes { get; init; } = [];
    public int VolunteersRequired { get; init; }
    public int VolunteersFilled { get; init; }
    public string? MinQualifications { get; init; }
    public string? AdditionalComments { get; init; }
    public ProjectStatus Status { get; init; }
    public string FacultyName { get; init; } = null!;
    public string Affiliation { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? UserName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
