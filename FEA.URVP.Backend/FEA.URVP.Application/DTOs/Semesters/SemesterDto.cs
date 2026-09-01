namespace FEA.URVP.Application.DTOs.Semesters;

public sealed class SemesterDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public DateTime? ApplicationWindowStart { get; init; }
    public DateTime? ApplicationWindowEnd { get; init; }
    public bool IsApplicationWindowOpen { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
