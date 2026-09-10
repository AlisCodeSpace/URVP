using System.ComponentModel.DataAnnotations;

namespace FEA.URVP.Domain.Entities.HomeIntro;

/// <summary>
/// Singleton row for the homepage intro section (headline, description, and key points).
/// </summary>
public class HomeIntro
{
    public const int HeadlineMaxLength = 200;
    public const int DescriptionMaxLength = 8000;
    public const int KeyPointMaxLength = 400;
    public const int MaxKeyPoints = 5;

    /// <summary>Only one HomeIntro row exists.</summary>
    public static readonly Guid SingletonId = Guid.Parse("b7e4c2a1-6d38-4f91-8c05-1a9e3b72d4f8");

    [Key]
    public Guid Id { get; set; } = SingletonId;

    [Required, MaxLength(HeadlineMaxLength)]
    public string Headline { get; set; } = null!;

    [Required, MaxLength(DescriptionMaxLength)]
    public string Description { get; set; } = null!;

    public List<string> KeyPoints { get; set; } = [];

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? UpdatedByUserId { get; set; }
}
