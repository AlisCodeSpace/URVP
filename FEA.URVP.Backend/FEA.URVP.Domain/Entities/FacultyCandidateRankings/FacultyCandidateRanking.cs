using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Domain.Entities.FacultyCandidateRankings;

/// <summary>
/// A faculty member's ranked preference for a student who applied to their project
/// (1 = first choice through 3 = third choice). Ranks are preference tiers, so several
/// candidates may share a tier; seat limits are enforced by matching, not here.
/// </summary>
public class FacultyCandidateRanking
{
    public const byte MinRank = 1;
    public const byte MaxRank = 3;

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    [Required]
    public Guid StudentUserId { get; set; }

    public User StudentUser { get; set; } = null!;

    /// <summary>Preference order: 1 (first choice) is highest.</summary>
    [Required]
    public byte Rank { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
