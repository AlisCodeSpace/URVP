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
        string label,
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
                $"{label} cannot open before the academic cycle starts.");
        }

        if (cycleEnd.HasValue && windowEnd.HasValue && windowEnd.Value > cycleEnd.Value)
        {
            throw new ArgumentException(
                $"{label} cannot close after the academic cycle ends.");
        }
    }

    public static void EnsureWindowsDoNotOverlap(
        DateTime? applicationStart,
        DateTime? applicationEnd,
        DateTime? registrationStart,
        DateTime? registrationEnd)
    {
        if (Semester.RangesOverlap(
                applicationStart,
                applicationEnd,
                registrationStart,
                registrationEnd))
        {
            throw new ArgumentException(
                "The registration window and the application window cannot overlap.");
        }
    }

    public static void EnsureCycleActiveToOpen(Semester semester, DateTime? start, DateTime? end, DateTime utcNow, string windowName)
    {
        if (!semester.IsCycleActive(utcNow)
            && start.HasValue
            && Semester.IsWithin(start, end, utcNow))
        {
            throw new InvalidOperationException(
                $"Start the academic cycle before opening {windowName}.");
        }
    }

    public static async Task EnsureNoOtherActiveCycleAsync(
        ISemesterRepository semesters,
        Guid? exceptId,
        CancellationToken cancellationToken)
    {
        var active = await semesters.FindActiveAsync(cancellationToken);
        if (active is null)
            return;
        if (exceptId.HasValue && active.Id == exceptId.Value)
            return;

        throw new InvalidOperationException(
            $"\"{active.Name}\" is still active. End that cycle before creating or starting another.");
    }

    public static void EnsureNotEnded(Semester semester, DateTime utcNow)
    {
        if (semester.HasEnded(utcNow))
        {
            throw new InvalidOperationException(
                "Ended cycles are read-only and cannot be changed.");
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
            $"This cycle overlaps \"{other.Name}\". Choose dates after that cycle ended.");
    }

    public static async Task ApplyAsync(
        ISemesterRepository semesters,
        Semester semester,
        DateTime? cycleStart,
        DateTime? cycleEnd,
        DateTime? windowStart,
        DateTime? windowEnd,
        DateTime? registrationStart,
        DateTime? registrationEnd,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        EnsureRange("Academic cycle", cycleStart, cycleEnd);
        EnsureRange("Registration window", registrationStart, registrationEnd);
        EnsureRange("Application window", windowStart, windowEnd);
        EnsureWindowWithinCycle(
            "The registration window", cycleStart, cycleEnd, registrationStart, registrationEnd);
        EnsureWindowWithinCycle(
            "The application window", cycleStart, cycleEnd, windowStart, windowEnd);
        EnsureWindowsDoNotOverlap(windowStart, windowEnd, registrationStart, registrationEnd);
        await EnsureNoCycleOverlapAsync(
            semesters, semester.Id, cycleStart, cycleEnd, cancellationToken);

        semester.ApplyCycleDates(cycleStart, cycleEnd, utcNow);
        semester.ApplyRegistrationWindow(registrationStart, registrationEnd, utcNow);
        semester.ApplyApplicationWindow(windowStart, windowEnd, utcNow);
        EnsureCycleActiveToOpen(semester, registrationStart, registrationEnd, utcNow, "registration");
        EnsureCycleActiveToOpen(semester, windowStart, windowEnd, utcNow, "applications");

        if (semester.IsCycleActive(utcNow))
            await semesters.RelinquishAllExceptAsync(semester.Id, utcNow, cancellationToken);
    }
}
