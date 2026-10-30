using FEA.URVP.Application.DTOs.ValueLists;
using FEA.URVP.Domain.Entities.ValueLists;

namespace FEA.URVP.Application.Mappings;

public static class ValueListMappings
{
    public static ValueListItemDto ToDto(this ValueListItem item) => new()
    {
        Id = item.Id,
        Kind = item.Kind,
        Name = item.Name,
        SortOrder = item.SortOrder,
        IsActive = item.IsActive,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };
}
