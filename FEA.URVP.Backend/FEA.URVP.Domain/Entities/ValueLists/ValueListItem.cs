using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Domain.Entities.ValueLists;

public class ValueListItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public ValueListKind Kind { get; set; }

    [Required, MaxLength(256)]
    public string Name { get; set; } = null!;

    [Required]
    public int SortOrder { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
