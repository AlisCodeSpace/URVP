namespace FEA.URVP.Application.DTOs.Email;

public sealed class EmailLogDto
{
    public Guid Id { get; init; }

    public string From { get; init; } = string.Empty;

    public string To { get; init; } = string.Empty;

    public string? Cc { get; init; }

    public string? Bcc { get; init; }

    public string? Exception { get; init; }

    public bool Success { get; init; }

    public DateTime CreatedOn { get; init; }
}
