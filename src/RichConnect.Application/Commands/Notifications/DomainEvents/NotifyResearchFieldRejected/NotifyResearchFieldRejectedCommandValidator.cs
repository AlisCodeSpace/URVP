using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldRejected
{
    public class NotifyResearchFieldRejectedCommandValidator : AbstractValidator<NotifyResearchFieldRejectedCommand>
    {
        public NotifyResearchFieldRejectedCommandValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty()
                .WithMessage("Field ID is required");

            RuleFor(x => x.RejectedByUserId)
                .NotEmpty()
                .WithMessage("Rejected by user ID is required");

            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("Rejection reason is required");
        }
    }
}
