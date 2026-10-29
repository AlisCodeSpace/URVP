using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Domain.Entities.Projects;

/// <summary>
/// Someone listed to help review a project. They do not take a volunteer seat.
/// They can open the project and rank students, and they cannot edit the listing.
/// Identified only by <see cref="Email"/> (stored lowercase), so the person can be
/// added before they have ever signed in; their name comes from their account once they have.
/// </summary>
public class ProjectResearchAssistant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    /// <summary>Lowercase, trimmed. Matches the lowercase email stored at sign-in.</summary>
    [Required, MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    public Guid AddedByUserId { get; set; }

    public User AddedByUser { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
