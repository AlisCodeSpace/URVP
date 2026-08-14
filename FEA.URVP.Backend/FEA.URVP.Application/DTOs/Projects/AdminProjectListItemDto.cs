using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.DTOs.Projects;

public sealed class AdminProjectListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string FacultyName { get; init; } = null!;
    public string Affiliation { get; init; } = null!;
    public string Email { get; init; } = null!;
    public ProjectStatus Status { get; init; }
    public int VolunteersRequired { get; init; }
    public int VolunteersFilled { get; init; }
    public int RankingCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
