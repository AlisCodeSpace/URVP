namespace FEA.URVP.Application.DTOs.Notifications;

public sealed class NotificationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Type { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? Data { get; init; }
    public Guid? ReferenceId { get; init; }
    public string? ReferenceType { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public string Priority { get; init; } = null!;
}
