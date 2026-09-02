using System.ComponentModel.DataAnnotations;

namespace FEA.URVP.Domain.Entities.Semesters;

/// <summary>
/// Represents an academic semester / program cycle.
/// The cycle and the student application window are each scheduled with
/// start/end dates: they open at the start, close automatically at the end,
/// and can also be ended immediately by an admin.
/// </summary>
public class Semester
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Display name, e.g. "Fall 2025–26".</summary>
    [Required, MaxLength(256)]
    public string Name { get; set; } = null!;

    /// <summary>Optional description or notes visible to admins.</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Persisted flag for the designated running cycle. Kept in sync with
    /// <see cref="CycleStart"/> / <see cref="CycleEnd"/> on write, and also
    /// evaluated from those dates at read time so a cycle auto-closes when
    /// its end is reached without a background job.
    /// Only one semester should be the designated cycle at a time.
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>UTC moment when this academic cycle begins. Null if not scheduled.</summary>
    public DateTime? CycleStart { get; set; }

    /// <summary>UTC moment when this academic cycle ends. Null until scheduled or ended.</summary>
    public DateTime? CycleEnd { get; set; }

    /// <summary>UTC moment when the student application window opens. Null if not yet set.</summary>
    public DateTime? ApplicationWindowStart { get; set; }

    /// <summary>UTC moment when the student application window closes. Null if not yet set.</summary>
    public DateTime? ApplicationWindowEnd { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// True when <paramref name="utcNow"/> falls in [start, end). A missing
    /// end means the interval stays open until it is closed or an end is set.
    /// </summary>
    public static bool IsWithin(DateTime? start, DateTime? end, DateTime utcNow) =>
        start.HasValue
        && utcNow >= start.Value
        && (!end.HasValue || utcNow < end.Value);

    /// <summary>True when this academic cycle is currently running.</summary>
    public bool IsCycleActive(DateTime utcNow) =>
        IsWithin(CycleStart, CycleEnd, utcNow)
        || (!CycleStart.HasValue && IsActive);

    /// <summary>
    /// True when students may submit applications: the cycle is running and
    /// the current UTC time falls within the application window.
    /// </summary>
    public bool IsApplicationWindowOpen(DateTime utcNow) =>
        IsCycleActive(utcNow)
        && IsWithin(ApplicationWindowStart, ApplicationWindowEnd, utcNow);

    /// <summary>True when two half-open intervals overlap.</summary>
    public static bool RangesOverlap(
        DateTime? aStart,
        DateTime? aEnd,
        DateTime? bStart,
        DateTime? bEnd)
    {
        if (!aStart.HasValue || !bStart.HasValue)
            return false;

        var aUntil = aEnd ?? DateTime.MaxValue;
        var bUntil = bEnd ?? DateTime.MaxValue;
        return aStart.Value < bUntil && bStart.Value < aUntil;
    }

    public void ApplyCycleDates(DateTime? start, DateTime? end, DateTime utcNow)
    {
        CycleStart = start;
        CycleEnd = end;
        IsActive = IsCycleActive(utcNow);
        UpdatedAt = utcNow;
    }

    public void ApplyApplicationWindow(DateTime? start, DateTime? end, DateTime utcNow)
    {
        ApplicationWindowStart = start;
        ApplicationWindowEnd = end;
        UpdatedAt = utcNow;
    }

    /// <summary>Start the cycle immediately, keeping a future end date if one is set.</summary>
    public void StartCycleNow(DateTime utcNow)
    {
        CycleStart = utcNow;
        if (CycleEnd.HasValue && CycleEnd.Value <= utcNow)
            CycleEnd = null;
        IsActive = true;
        UpdatedAt = utcNow;
    }

    /// <summary>End the cycle immediately and close an open application window.</summary>
    public void EndCycleNow(DateTime utcNow)
    {
        if (!CycleStart.HasValue || CycleStart.Value > utcNow)
        {
            CycleStart = utcNow;
        }

        CycleEnd = utcNow;
        CloseApplicationWindowNow(utcNow);
        IsActive = false;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// Stop this semester from remaining the running/upcoming cycle so another
    /// can take its place.
    /// </summary>
    public void RelinquishCycle(DateTime utcNow)
    {
        if (!CycleStart.HasValue || CycleStart.Value > utcNow)
        {
            CycleStart = null;
            CycleEnd = null;
        }
        else if (!CycleEnd.HasValue || CycleEnd.Value > utcNow)
        {
            CycleEnd = utcNow;
            CloseApplicationWindowNow(utcNow);
        }

        IsActive = false;
        UpdatedAt = utcNow;
    }

    public void OpenApplicationWindowNow(DateTime utcNow)
    {
        ApplicationWindowStart = utcNow;
        if (ApplicationWindowEnd.HasValue && ApplicationWindowEnd.Value <= utcNow)
            ApplicationWindowEnd = null;
        UpdatedAt = utcNow;
    }

    public void CloseApplicationWindowNow(DateTime utcNow)
    {
        if (!ApplicationWindowStart.HasValue)
            ApplicationWindowStart = utcNow;

        if (!ApplicationWindowEnd.HasValue || ApplicationWindowEnd.Value > utcNow)
            ApplicationWindowEnd = utcNow;

        UpdatedAt = utcNow;
    }
}
