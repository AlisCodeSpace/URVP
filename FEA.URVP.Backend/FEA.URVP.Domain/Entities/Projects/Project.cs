using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Domain.Entities.Projects;

public class Project
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = null!;

    /// <summary>Selected research-area labels (1–6), persisted as JSON.</summary>
    [Required]
    public List<string> ResearchAreas { get; set; } = [];

    [Required]
    public IrbStage IrbStage { get; set; }

    [Required, MaxLength(4000)]
    public string BriefDescription { get; set; } = null!;

    /// <summary>Selected activity-type labels (1–6), persisted as JSON.</summary>
    [Required]
    public List<string> ActivityTypes { get; set; } = [];

    [Required]
    public int VolunteersRequired { get; set; }

    [Required]
    public int VolunteersFilled { get; set; }

    [MaxLength(2000)]
    public string? MinQualifications { get; set; }

    [MaxLength(2000)]
    public string? AdditionalComments { get; set; }

    [Required]
    public ProjectStatus Status { get; set; } = ProjectStatus.Open;

    [Required, MaxLength(128)]
    public string FacultyNameSnapshot { get; set; } = null!;

    [Required, MaxLength(256)]
    public string AffiliationSnapshot { get; set; } = null!;

    [Required, MaxLength(256)]
    public string EmailSnapshot { get; set; } = null!;

    [MaxLength(64)]
    public string? UserNameSnapshot { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
