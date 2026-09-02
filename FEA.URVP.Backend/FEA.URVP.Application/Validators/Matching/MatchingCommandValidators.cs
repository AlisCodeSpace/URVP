using FEA.URVP.Application.Commands.Matching.UpdatePlacementStatus;
using FEA.URVP.Domain.Enums;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Matching;

public sealed class UpdatePlacementStatusCommandValidator
    : AbstractValidator<UpdatePlacementStatusCommand>
{
    public UpdatePlacementStatusCommandValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s is PlacementStatus.Declined or PlacementStatus.Cancelled)
            .WithMessage("Status must be Declined or Cancelled.");
    }
}
