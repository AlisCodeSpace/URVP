using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.StudentProfiles;
using MediatR;

namespace FEA.URVP.Application.Commands.StudentProfiles.Upsert;

public sealed class DayAvailabilityInput
{
    public string Day { get; init; } = null!;
    public List<string> Slots { get; init; } = [];
}

public sealed class UpsertStudentProfileCommand : IRequest<StudentProfileDto>
{
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }

    public string Gender { get; init; } = null!;
    public string MobileNumber { get; init; } = null!;
    public string Degree { get; init; } = null!;
    public int ExpectedGraduationYear { get; init; }
    public List<string> Languages { get; init; } = [];
    public string? OtherLanguages { get; init; }
    public bool CompletedCredits { get; init; }
    public decimal CumulativeAverage { get; init; }
    public List<string> ResearchTopics { get; init; } = [];
    public string? Publications { get; init; }
    public Guid TranscriptFileId { get; init; }
    public Guid? CitiFileId { get; init; }
    public List<DayAvailabilityInput> Availability { get; init; } = [];
}
