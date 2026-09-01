using System.ComponentModel.DataAnnotations;

namespace FEA.URVP.Domain.Entities.Semesters;

/// <summary>
/// Represents an academic semester / program cycle.
/// A semester is "active" when it is the current running cycle.
/// The application window controls when students can submit project applications.
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
    /// Whether this is the currently running academic cycle.
    /// Only one semester should be active at a time; the repository
    /// enforces this by deactivating all others when one is activated.
    /// </summary>
    [Required]
    public bool IsActive { get; set; }

    /// <summary>UTC moment when the student application window opens. Null if not yet set.</summary>
    public DateTime? ApplicationWindowStart { get; set; }

    /// <summary>UTC moment when the student application window closes. Null if the window is still open.</summary>
    public DateTime? ApplicationWindowEnd { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Computed helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the application window is currently open.
    /// Requires the semester to be active and the current UTC time to fall
    /// within [ApplicationWindowStart, ApplicationWindowEnd).
    /// </summary>
    public bool IsApplicationWindowOpen(DateTime utcNow) =>
        IsActive
        && ApplicationWindowStart.HasValue
        && utcNow >= ApplicationWindowStart.Value
        && (!ApplicationWindowEnd.HasValue || utcNow < ApplicationWindowEnd.Value);
}
