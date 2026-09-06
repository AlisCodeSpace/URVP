using System.ComponentModel.DataAnnotations;
using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Domain.Entities.StudentProfiles;

public class StudentProfile
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    [Required, MaxLength(16)]
    public string Gender { get; set; } = null!;

    [Required, MaxLength(32)]
    public string MobileNumber { get; set; } = null!;

    [Required, MaxLength(32)]
    public string Degree { get; set; } = null!;

    [Required]
    public int ExpectedGraduationYear { get; set; }

    /// <summary>Selected language labels, persisted as JSON.</summary>
    [Required]
    public List<string> Languages { get; set; } = [];

    [MaxLength(256)]
    public string? OtherLanguages { get; set; }

    /// <summary>
    /// Minimum cumulative average (AUB 100-point scale) for the dashboard
    /// "profile ready" count. Aligns with the published 3.0 / 78 eligibility copy.
    /// </summary>
    public const decimal MinimumCumulativeAverage = 78m;

    /// <summary>True when the student has completed at least 24 credits at AUB.</summary>
    [Required]
    public bool CompletedCredits { get; set; }

    [Required]
    public decimal CumulativeAverage { get; set; }

    /// <summary>Selected research-topic labels (0–6), persisted as JSON.</summary>
    [Required]
    public List<string> ResearchTopics { get; set; } = [];

    [MaxLength(4000)]
    public string? Publications { get; set; }

    /// <summary>FileStorage Id for the uploaded transcript PDF.</summary>
    public Guid? TranscriptFileId { get; set; }

    /// <summary>FileStorage Id for the uploaded CITI certification PDF.</summary>
    public Guid? CitiFileId { get; set; }

    /// <summary>Weekly availability entries, persisted as JSON.</summary>
    [Required]
    public List<DayAvailability> Availability { get; set; } = [];

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
