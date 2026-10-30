using FEA.URVP.Application.Features.Workshops.DTOs;
using FEA.URVP.Domain.Entities.Workshops;

namespace FEA.URVP.Application.Features.Workshops.Mappings;

public static class WorkshopMappings
{
    public static WorkshopDto ToDto(this Workshop workshop) => new()
    {
        Id = workshop.Id,
        Title = workshop.Title,
        Date = workshop.Date,
        Time = workshop.Time,
        Location = workshop.Location,
        Description = workshop.Description,
        RegistrationUrl = workshop.RegistrationUrl,
        PosterFileId = workshop.PosterFileId,
        PosterAlt = workshop.PosterAlt,
        Published = workshop.Published,
        SortOrder = workshop.SortOrder,
        CreatedAt = workshop.CreatedAt,
        UpdatedAt = workshop.UpdatedAt,
    };
}
