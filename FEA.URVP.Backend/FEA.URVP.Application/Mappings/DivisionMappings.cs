using FEA.URVP.Application.DTOs.Divisions;
using FEA.URVP.Domain.Entities.Divisions;

namespace FEA.URVP.Application.Mappings;

public static class DivisionMappings
{
    public static DivisionDto ToDto(this Division division) => new()
    {
        Id = division.Id,
        Name = division.Name,
        Description = division.Description,
        IsActive = division.IsActive,
        CreatedAt = division.CreatedAt,
        UpdatedAt = division.UpdatedAt
    };
}
