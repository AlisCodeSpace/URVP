using FEA.URVP.Application.Commands.StudentProfiles.Upsert;
using FEA.URVP.Domain.Catalog;
using FluentValidation;

namespace FEA.URVP.Application.Validators.StudentProfiles;

public sealed class UpsertStudentProfileCommandValidator
    : AbstractValidator<UpsertStudentProfileCommand>
{
    public UpsertStudentProfileCommandValidator()
    {
        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .Must(StudentProfileCatalog.Genders.Contains)
            .WithMessage("Gender is not allowed.");

        RuleFor(x => x.MobileNumber)
            .NotEmpty().WithMessage("Mobile number is required.")
            .MaximumLength(32);

        RuleFor(x => x.Degree)
            .NotEmpty().WithMessage("Degree is required.")
            .Must(StudentProfileCatalog.Degrees.Contains)
            .WithMessage("Degree is not allowed.");

        var year = DateTime.UtcNow.Year;
        RuleFor(x => x.ExpectedGraduationYear)
            .InclusiveBetween(year, year + 10)
            .WithMessage($"Expected graduation year must be between {year} and {year + 10}.");

        RuleFor(x => x.Languages)
            .Must(list => list.Count <= StudentProfileCatalog.MaxLanguages)
            .WithMessage($"Select at most {StudentProfileCatalog.MaxLanguages} languages.")
            .Must(list => list.Distinct(StringComparer.Ordinal).Count() == list.Count)
            .WithMessage("Languages must be unique.")
            .Must(list => list.All(StudentProfileCatalog.Languages.Contains))
            .WithMessage("One or more languages are not allowed.");

        RuleFor(x => x.OtherLanguages)
            .MaximumLength(256)
            .When(x => x.OtherLanguages is not null);

        RuleFor(x => x.CumulativeAverage)
            .Must(IsValidAverage)
            .WithMessage("Cumulative average must be on a 0–4.0 or 0–100 scale.");

        RuleFor(x => x.ResearchTopics)
            .Must(list => list.Count <= StudentProfileCatalog.MaxResearchTopics)
            .WithMessage($"Select at most {StudentProfileCatalog.MaxResearchTopics} research topics.")
            .Must(list => list.Distinct(StringComparer.Ordinal).Count() == list.Count)
            .WithMessage("Research topics must be unique.")
            .Must(list => list.All(ResearchAreaCatalog.Allowed.Contains))
            .WithMessage("One or more research topics are not allowed.");

        RuleFor(x => x.Publications)
            .MaximumLength(4000)
            .When(x => x.Publications is not null);

        RuleFor(x => x.TranscriptFileId)
            .NotEmpty().WithMessage("Transcript file is required.");

        RuleFor(x => x.CitiFileId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("CITI file id is invalid.");

        RuleForEach(x => x.Availability).ChildRules(day =>
        {
            day.RuleFor(d => d.Day)
                .NotEmpty()
                .Must(StudentProfileCatalog.Weekdays.Contains)
                .WithMessage("Availability day is not allowed.");

            day.RuleFor(d => d.Slots)
                .Must(slots => slots.Distinct(StringComparer.Ordinal).Count() == slots.Count)
                .WithMessage("Availability slots must be unique for each day.")
                .Must(slots => slots.All(StudentProfileCatalog.TimeSlots.Contains))
                .WithMessage("One or more availability slots are not allowed.");
        });
    }

    private static bool IsValidAverage(decimal value) =>
        (value >= 0m && value <= 4m) || (value > 4m && value <= 100m);
}
