using FEA.URVP.Application.DTOs.Semesters;
using FEA.URVP.Domain.Entities.Semesters;

namespace FEA.URVP.Application.Mappings;

public static class SemesterMappings
{
    public static SemesterDto ToDto(this Semester semester) => new()
    {
        Id = semester.Id,
        Name = semester.Name,
        Description = semester.Description,
        IsActive = semester.IsActive,
        ApplicationWindowStart = semester.ApplicationWindowStart,
        ApplicationWindowEnd = semester.ApplicationWindowEnd,
        IsApplicationWindowOpen = semester.IsApplicationWindowOpen(DateTime.UtcNow),
        CreatedAt = semester.CreatedAt,
        UpdatedAt = semester.UpdatedAt,
    };
}
