using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerApproved
{
    public class NotifyPartnerApprovedCommandValidator : AbstractValidator<NotifyPartnerApprovedCommand>
    {
        public NotifyPartnerApprovedCommandValidator()
        {
            RuleFor(x => x.PartnerId)
                .NotEmpty()
                .WithMessage("Partner ID is required");

            RuleFor(x => x.ApprovedByUserId)
                .NotEmpty()
                .WithMessage("Approved by user ID is required");
        }
    }
}
