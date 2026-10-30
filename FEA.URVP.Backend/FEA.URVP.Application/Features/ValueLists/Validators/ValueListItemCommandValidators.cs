using FEA.URVP.Application.Commands.ValueLists.Create;
using FEA.URVP.Application.Commands.ValueLists.Update;
using FluentValidation;

namespace FEA.URVP.Application.Validators.ValueLists;

public sealed class CreateValueListItemCommandValidator
    : AbstractValidator<CreateValueListItemCommand>
{
    public CreateValueListItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256);
    }
}

public sealed class UpdateValueListItemCommandValidator
    : AbstractValidator<UpdateValueListItemCommand>
{
    public UpdateValueListItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256);
    }
}
