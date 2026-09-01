namespace FEA.URVP.Application.DTOs.Workshops;

public sealed class WorkshopDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string Date { get; init; } = null!;
    public string? Time { get; init; }
    public string? Location { get; init; }
    public string Description { get; init; } = null!;
    public string RegistrationUrl { get; init; } = null!;
    public Guid? PosterFileId { get; init; }
    public string? PosterAlt { get; init; }
    public int SortOrder { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
