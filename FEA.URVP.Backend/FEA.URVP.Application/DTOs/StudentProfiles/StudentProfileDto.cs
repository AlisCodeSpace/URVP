namespace FEA.URVP.Application.DTOs.StudentProfiles;

public sealed class DayAvailabilityDto
{
    public string Day { get; init; } = null!;
    public IReadOnlyList<string> Slots { get; init; } = [];
}

public sealed class StudentProfileDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public bool Exists { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? Gender { get; init; }
    public string? MobileNumber { get; init; }
    public string? Degree { get; init; }
    public int? ExpectedGraduationYear { get; init; }
    public IReadOnlyList<string> Languages { get; init; } = [];
    public string? OtherLanguages { get; init; }
    public bool? CompletedCredits { get; init; }
    public decimal? CumulativeAverage { get; init; }
    public IReadOnlyList<string> ResearchTopics { get; init; } = [];
    public string? Publications { get; init; }
    public Guid? TranscriptFileId { get; init; }
    public string? TranscriptFileName { get; init; }
    public Guid? CitiFileId { get; init; }
    public string? CitiFileName { get; init; }
    public IReadOnlyList<DayAvailabilityDto> Availability { get; init; } = [];
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
