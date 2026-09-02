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
        IsActive = semester.IsCycleActive(DateTime.UtcNow),
        CycleStart = AsUtc(semester.CycleStart),
        CycleEnd = AsUtc(semester.CycleEnd),
        ApplicationWindowStart = AsUtc(semester.ApplicationWindowStart),
        ApplicationWindowEnd = AsUtc(semester.ApplicationWindowEnd),
        IsApplicationWindowOpen = semester.IsApplicationWindowOpen(DateTime.UtcNow),
        CreatedAt = AsUtc(semester.CreatedAt),
        UpdatedAt = AsUtc(semester.UpdatedAt),
    };

    /// <summary>
    /// SQL Server datetime2 values come back Unspecified; mark them UTC so JSON includes Z.
    /// </summary>
    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static DateTime? AsUtc(DateTime? value) =>
        value.HasValue ? AsUtc(value.Value) : null;
}
