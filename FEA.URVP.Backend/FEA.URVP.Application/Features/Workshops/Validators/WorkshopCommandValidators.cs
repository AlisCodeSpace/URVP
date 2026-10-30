using FEA.URVP.Application.Commands.Workshops.Create;
using FEA.URVP.Application.Commands.Workshops.Update;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Workshops;

public sealed class CreateWorkshopCommandValidator
    : AbstractValidator<CreateWorkshopCommand>
{
    public CreateWorkshopCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(256);

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .MaximumLength(64);

        RuleFor(x => x.Time)
            .MaximumLength(64)
            .When(x => x.Time is not null);

        RuleFor(x => x.Location)
            .MaximumLength(256)
            .When(x => x.Location is not null);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000);

        RuleFor(x => x.RegistrationUrl)
            .NotEmpty().WithMessage("Registration URL is required.")
            .MaximumLength(500);

        RuleFor(x => x.PosterAlt)
            .MaximumLength(256)
            .When(x => x.PosterAlt is not null);
    }
}

public sealed class UpdateWorkshopCommandValidator
    : AbstractValidator<UpdateWorkshopCommand>
{
    public UpdateWorkshopCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(256);

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .MaximumLength(64);

        RuleFor(x => x.Time)
            .MaximumLength(64)
            .When(x => x.Time is not null);

        RuleFor(x => x.Location)
            .MaximumLength(256)
            .When(x => x.Location is not null);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000);

        RuleFor(x => x.RegistrationUrl)
            .NotEmpty().WithMessage("Registration URL is required.")
            .MaximumLength(500);

        RuleFor(x => x.PosterAlt)
            .MaximumLength(256)
            .When(x => x.PosterAlt is not null);
    }
}
