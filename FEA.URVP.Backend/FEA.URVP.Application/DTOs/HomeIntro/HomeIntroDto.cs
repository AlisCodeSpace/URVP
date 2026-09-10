namespace FEA.URVP.Application.DTOs.HomeIntro;

public sealed class HomeIntroDto
{
    public string Headline { get; init; } = null!;

    public string Description { get; init; } = null!;

    public IReadOnlyList<string> KeyPoints { get; init; } = [];

    public DateTime UpdatedAt { get; init; }
}
