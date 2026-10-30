using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.DTOs.ValueLists;

public sealed class ValueListItemDto
{
    public Guid Id { get; init; }
    public ValueListKind Kind { get; init; }
    public string Name { get; init; } = null!;
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
