using FEA.URVP.Application.Commands.HomeIntro.Update;
using FluentValidation;
using HomeIntroRow = FEA.URVP.Domain.Entities.HomeIntro.HomeIntro;

namespace FEA.URVP.Application.Validators.HomeIntro;

public sealed class UpdateHomeIntroCommandValidator
    : AbstractValidator<UpdateHomeIntroCommand>
{
    public UpdateHomeIntroCommandValidator()
    {
        RuleFor(x => x.Headline)
            .NotEmpty().WithMessage("Headline is required.")
            .MaximumLength(HomeIntroRow.HeadlineMaxLength);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(HomeIntroRow.DescriptionMaxLength);

        RuleFor(x => x.KeyPoints)
            .NotNull()
            .Must(points => points.Count <= HomeIntroRow.MaxKeyPoints)
            .WithMessage($"No more than {HomeIntroRow.MaxKeyPoints} key points are allowed.");

        RuleForEach(x => x.KeyPoints)
            .Must(point => string.IsNullOrWhiteSpace(point)
                || point.Trim().Length <= HomeIntroRow.KeyPointMaxLength)
            .WithMessage($"Each key point must be {HomeIntroRow.KeyPointMaxLength} characters or fewer.");
    }
}
