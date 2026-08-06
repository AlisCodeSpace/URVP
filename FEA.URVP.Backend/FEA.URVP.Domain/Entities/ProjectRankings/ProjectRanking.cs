using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Domain.Entities.ProjectRankings;

/// <summary>
/// A student's ranked preference for a project (1 = highest). At most 3 per student.
/// </summary>
public class ProjectRanking
{
    public const byte MinRank = 1;
    public const byte MaxRank = 3;

    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid StudentUserId { get; set; }

    public User StudentUser { get; set; } = null!;

    [Required]
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    /// <summary>Preference order: 1 (first choice) through 3 (third choice).</summary>
    [Required]
    public byte Rank { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
