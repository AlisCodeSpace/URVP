using FEA.URVP.Application.Features.ValueLists.Commands.Create;
using FEA.URVP.Application.Features.ValueLists.Commands.Update;
using FluentValidation;

namespace FEA.URVP.Application.Features.ValueLists.Validators;

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
