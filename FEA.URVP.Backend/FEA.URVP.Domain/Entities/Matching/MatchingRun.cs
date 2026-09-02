using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Domain.Entities.Matching;

/// <summary>
/// One execution of the automatic matching algorithm for a semester.
/// A run is created as a draft, reviewed, then confirmed or discarded.
/// Inputs that affect reproducibility (seed, algorithm version) are stored with it.
/// </summary>
public class MatchingRun
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SemesterId { get; set; }

    public Semester Semester { get; set; } = null!;

    [Required]
    public MatchingRunStatus Status { get; set; } = MatchingRunStatus.Draft;

    /// <summary>Identifies the algorithm and tie-break policy used, e.g. "da-student-proposing/v1".</summary>
    [Required, MaxLength(64)]
    public string AlgorithmVersion { get; set; } = null!;

    /// <summary>Seed of the deterministic lottery used to break remaining ties.</summary>
    [Required]
    public int Seed { get; set; }

    public int StudentsConsidered { get; set; }

    public int ProjectsConsidered { get; set; }

    public int SeatsAvailable { get; set; }

    public int StudentsMatched { get; set; }

    public int TieBreaksUsed { get; set; }

    /// <summary>Validation notes surfaced to the admin, persisted as JSON.</summary>
    [Required]
    public List<string> Warnings { get; set; } = [];

    [Required]
    public Guid CreatedByUserId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? ConfirmedByUserId { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public List<Placement> Placements { get; set; } = [];

    public void Confirm(Guid adminUserId, DateTime utcNow)
    {
        if (Status != MatchingRunStatus.Draft)
        {
            throw new InvalidOperationException("Only draft runs can be confirmed.");
        }

        Status = MatchingRunStatus.Confirmed;
        ConfirmedByUserId = adminUserId;
        ConfirmedAt = utcNow;

        foreach (var placement in Placements)
        {
            placement.SetStatus(PlacementStatus.Confirmed, utcNow);
        }
    }

    public void Discard(DateTime utcNow)
    {
        if (Status != MatchingRunStatus.Draft)
        {
            throw new InvalidOperationException("Only draft runs can be discarded.");
        }

        Status = MatchingRunStatus.Discarded;

        foreach (var placement in Placements)
        {
            placement.SetStatus(PlacementStatus.Cancelled, utcNow);
        }
    }
}
