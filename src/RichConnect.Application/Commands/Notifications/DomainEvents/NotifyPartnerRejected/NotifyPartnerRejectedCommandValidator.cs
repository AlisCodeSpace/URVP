using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerRejected
{
    public class NotifyPartnerRejectedCommandValidator : AbstractValidator<NotifyPartnerRejectedCommand>
    {
        public NotifyPartnerRejectedCommandValidator()
        {
            RuleFor(x => x.PartnerId)
                .NotEmpty()
                .WithMessage("Partner ID is required");

            RuleFor(x => x.RejectedByUserId)
                .NotEmpty()
                .WithMessage("Rejected by user ID is required");

            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("Rejection reason is required");
        }
    }
}
