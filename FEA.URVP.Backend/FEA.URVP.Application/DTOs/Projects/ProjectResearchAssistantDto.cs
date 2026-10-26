namespace FEA.URVP.Application.DTOs.Projects;

public sealed class ProjectResearchAssistantDto
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public DateTime AddedAt { get; init; }
}
