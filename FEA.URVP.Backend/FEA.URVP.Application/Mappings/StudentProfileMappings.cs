using FEA.URVP.Application.DTOs.StudentProfiles;
using FEA.URVP.Domain.Entities.StudentProfiles;
using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Application.Mappings;

public static class StudentProfileMappings
{
    public static StudentProfileDto ToDto(
        this StudentProfile profile,
        User user,
        string? transcriptFileName = null,
        string? citiFileName = null)
    {
        var (firstName, lastName) = SplitName(user.Name);

        return new StudentProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Exists = true,
            FirstName = firstName,
            LastName = lastName,
            Email = user.Email,
            Gender = profile.Gender,
            MobileNumber = profile.MobileNumber,
            Degree = profile.Degree,
            ExpectedGraduationYear = profile.ExpectedGraduationYear,
            Languages = profile.Languages.ToList(),
            OtherLanguages = profile.OtherLanguages,
            CompletedCredits = profile.CompletedCredits,
            CumulativeAverage = profile.CumulativeAverage,
            ResearchTopics = profile.ResearchTopics.ToList(),
            Publications = profile.Publications,
            TranscriptFileId = profile.TranscriptFileId,
            TranscriptFileName = transcriptFileName,
            CitiFileId = profile.CitiFileId,
            CitiFileName = citiFileName,
            Availability = profile.Availability
                .Select(a => new DayAvailabilityDto
                {
                    Day = a.Day,
                    Slots = a.Slots.ToList(),
                })
                .ToList(),
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt,
        };
    }

    public static StudentProfileDto EmptyFromUser(User user)
    {
        var (firstName, lastName) = SplitName(user.Name);

        return new StudentProfileDto
        {
            Id = Guid.Empty,
            UserId = user.Id,
            Exists = false,
            FirstName = firstName,
            LastName = lastName,
            Email = user.Email,
            Languages = [],
            ResearchTopics = [],
            Availability = [],
        };
    }

    public static (string FirstName, string LastName) SplitName(string? fullName)
    {
        var parts = (fullName ?? string.Empty)
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return (string.Empty, string.Empty);
        }

        if (parts.Length == 1)
        {
            return (parts[0], string.Empty);
        }

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }
}
