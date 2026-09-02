using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Domain.Entities.Matching;

/// <summary>A student assigned to a project by a <see cref="MatchingRun"/>.</summary>
public class Placement
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid MatchingRunId { get; set; }

    public MatchingRun MatchingRun { get; set; } = null!;

    [Required]
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    [Required]
    public Guid StudentUserId { get; set; }

    public User StudentUser { get; set; } = null!;

    /// <summary>How the student ranked this project (1 = first choice).</summary>
    [Required]
    public byte StudentRank { get; set; }

    /// <summary>How the faculty ranked this student (1 = first choice).</summary>
    [Required]
    public byte FacultyRank { get; set; }

    /// <summary>True when the seeded lottery decided this seat over an equally ranked candidate.</summary>
    [Required]
    public bool ResolvedByTieBreak { get; set; }

    [Required]
    public PlacementStatus Status { get; set; } = PlacementStatus.Proposed;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool OccupiesSeat => Status == PlacementStatus.Confirmed;

    public void SetStatus(PlacementStatus status, DateTime utcNow)
    {
        Status = status;
        UpdatedAt = utcNow;
    }
}
