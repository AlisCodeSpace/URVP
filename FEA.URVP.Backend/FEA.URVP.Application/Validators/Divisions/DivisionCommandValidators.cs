using FEA.URVP.Application.Commands.Divisions.Create;
using FEA.URVP.Application.Commands.Divisions.Update;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Divisions;

public sealed class CreateDivisionCommandValidator
    : AbstractValidator<CreateDivisionCommand>
{
    public CreateDivisionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);
    }
}

public sealed class UpdateDivisionCommandValidator
    : AbstractValidator<UpdateDivisionCommand>
{
    public UpdateDivisionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);
    }
}
