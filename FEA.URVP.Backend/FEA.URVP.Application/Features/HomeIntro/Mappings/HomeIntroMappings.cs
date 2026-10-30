using FEA.URVP.Application.DTOs.HomeIntro;
using FEA.URVP.Domain.Catalog;
using HomeIntroRow = FEA.URVP.Domain.Entities.HomeIntro.HomeIntro;

namespace FEA.URVP.Application.Mappings;

public static class HomeIntroMappings
{
    public static HomeIntroDto ToDto(this HomeIntroRow intro) => new()
    {
        Headline = intro.Headline,
        Description = intro.Description,
        KeyPoints = intro.KeyPoints.ToList(),
        UpdatedAt = intro.UpdatedAt,
    };

    public static HomeIntroDto Defaults() => new()
    {
        Headline = HomeIntroCatalog.Headline,
        Description = HomeIntroCatalog.Description,
        KeyPoints = [.. HomeIntroCatalog.KeyPoints],
        UpdatedAt = DateTime.MinValue,
    };
}
