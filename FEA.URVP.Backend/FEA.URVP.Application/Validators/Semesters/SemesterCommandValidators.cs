using FEA.URVP.Application.Commands.Semesters.Create;
using FEA.URVP.Application.Commands.Semesters.SetApplicationWindow;
using FEA.URVP.Application.Commands.Semesters.Update;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Semesters;

public sealed class CreateSemesterCommandValidator
    : AbstractValidator<CreateSemesterCommand>
{
    public CreateSemesterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Semester name is required.")
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.CycleEnd)
            .GreaterThan(x => x.CycleStart)
            .WithMessage("Academic cycle end must be after the start date.")
            .When(x => x.CycleStart.HasValue && x.CycleEnd.HasValue);

        RuleFor(x => x.ApplicationWindowEnd)
            .GreaterThan(x => x.ApplicationWindowStart)
            .WithMessage("Application window end must be after the start date.")
            .When(x => x.ApplicationWindowStart.HasValue && x.ApplicationWindowEnd.HasValue);
    }
}

public sealed class UpdateSemesterCommandValidator
    : AbstractValidator<UpdateSemesterCommand>
{
    public UpdateSemesterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Semester name is required.")
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.CycleEnd)
            .GreaterThan(x => x.CycleStart)
            .WithMessage("Academic cycle end must be after the start date.")
            .When(x => x.CycleStart.HasValue && x.CycleEnd.HasValue);

        RuleFor(x => x.ApplicationWindowEnd)
            .GreaterThan(x => x.ApplicationWindowStart)
            .WithMessage("Application window end must be after the start date.")
            .When(x => x.ApplicationWindowStart.HasValue && x.ApplicationWindowEnd.HasValue);
    }
}

public sealed class SetApplicationWindowCommandValidator
    : AbstractValidator<SetApplicationWindowCommand>
{
    public SetApplicationWindowCommandValidator()
    {
        RuleFor(x => x.ApplicationWindowEnd)
            .GreaterThan(x => x.ApplicationWindowStart)
            .WithMessage("Application window end must be after the start date.")
            .When(x => x.ApplicationWindowStart.HasValue && x.ApplicationWindowEnd.HasValue);
    }
}
