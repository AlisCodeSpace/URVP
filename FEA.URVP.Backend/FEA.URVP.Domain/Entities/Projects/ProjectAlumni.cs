using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Semesters;

namespace FEA.URVP.Domain.Entities.Projects;

/// <summary>
/// A student stored when a project's cycle ends, so the posting faculty can
/// still see who took part after the live roster is cleared.
/// </summary>
public class ProjectAlumni
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    [Required]
    public Guid SemesterId { get; set; }

    public Semester Semester { get; set; } = null!;

    [Required, MaxLength(256)]
    public string SemesterName { get; set; } = null!;

    [Required]
    public Guid StudentUserId { get; set; }

    [Required, MaxLength(128)]
    public string StudentName { get; set; } = null!;

    [Required, MaxLength(256)]
    public string StudentEmail { get; set; } = null!;

    /// <summary>How the student ranked the project, when they applied.</summary>
    public byte? StudentRank { get; set; }

    /// <summary>How the faculty ranked the student, when they did.</summary>
    public byte? FacultyRank { get; set; }

    /// <summary>True when the student was confirmed onto the project that cycle.</summary>
    [Required]
    public bool WasConfirmed { get; set; }

    [Required]
    public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;
}
