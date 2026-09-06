using FEA.URVP.Application.Commands.Matching.Confirm;
using FEA.URVP.Application.Commands.Matching.UpdatePlacementStatus;
using FEA.URVP.Application.Commands.Notifications.NotifyMatchingConfirmed;
using FEA.URVP.Domain.Enums;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Matching;

public sealed class ConfirmMatchingRunCommandValidator : AbstractValidator<ConfirmMatchingRunCommand>
{
    public ConfirmMatchingRunCommandValidator()
    {
        RuleFor(x => x.RunId)
            .NotEmpty().WithMessage("RunId is required.");

        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("CurrentUserId is required.");
    }
}

public sealed class NotifyMatchingConfirmedCommandValidator
    : AbstractValidator<NotifyMatchingConfirmedCommand>
{
    public NotifyMatchingConfirmedCommandValidator()
    {
        RuleFor(x => x.RunId)
            .NotEmpty().WithMessage("RunId is required.");
    }
}

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
