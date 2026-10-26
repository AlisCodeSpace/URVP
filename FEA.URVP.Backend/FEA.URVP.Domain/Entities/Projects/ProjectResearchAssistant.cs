using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Domain.Entities.Projects;

/// <summary>
/// Someone helping run a project. Not a volunteer seat.
/// <see cref="UserId"/> is set when the person is chosen from a directory user;
/// a typed entry stores only the name and email.
/// </summary>
public class ProjectResearchAssistant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public Guid? UserId { get; set; }

    public User? User { get; set; }

    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    public Guid AddedByUserId { get; set; }

    public User AddedByUser { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
