using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Semesters;

namespace FEA.URVP.Application.Commands.Semesters;

internal static class SemesterSchedule
{
    public static void EnsureRange(string label, DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue && end.Value <= start.Value)
        {
            throw new ArgumentException($"{label} end must be after the start date.");
        }
    }

    public static void EnsureWindowWithinCycle(
        DateTime? cycleStart,
        DateTime? cycleEnd,
        DateTime? windowStart,
        DateTime? windowEnd)
    {
        if (!windowStart.HasValue)
            return;

        if (cycleStart.HasValue && windowStart.Value < cycleStart.Value)
        {
            throw new ArgumentException(
                "The application window cannot open before the academic cycle starts.");
        }

        if (cycleEnd.HasValue && windowEnd.HasValue && windowEnd.Value > cycleEnd.Value)
        {
            throw new ArgumentException(
                "The application window cannot close after the academic cycle ends.");
        }
    }

    public static async Task EnsureNoCycleOverlapAsync(
        ISemesterRepository semesters,
        Guid? excludeId,
        DateTime? start,
        DateTime? end,
        CancellationToken cancellationToken)
    {
        if (!start.HasValue)
            return;

        var other = await semesters.FindOverlappingCycleAsync(
            excludeId, start.Value, end, cancellationToken);
        if (other is null)
            return;

        throw new ArgumentException(
            $"This cycle overlaps \"{other.Name}\". End or shorten that cycle first, or pick different dates.");
    }

    public static async Task ApplyAsync(
        ISemesterRepository semesters,
        Semester semester,
        DateTime? cycleStart,
        DateTime? cycleEnd,
        DateTime? windowStart,
        DateTime? windowEnd,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        EnsureRange("Academic cycle", cycleStart, cycleEnd);
        EnsureRange("Application window", windowStart, windowEnd);
        EnsureWindowWithinCycle(cycleStart, cycleEnd, windowStart, windowEnd);
        await EnsureNoCycleOverlapAsync(
            semesters, semester.Id, cycleStart, cycleEnd, cancellationToken);

        semester.ApplyCycleDates(cycleStart, cycleEnd, utcNow);
        semester.ApplyApplicationWindow(windowStart, windowEnd, utcNow);

        if (semester.IsCycleActive(utcNow))
            await semesters.RelinquishAllExceptAsync(semester.Id, utcNow, cancellationToken);
    }
}
