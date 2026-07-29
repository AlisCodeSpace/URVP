using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerRegistered
{
    public class NotifyPartnerRegisteredCommandValidator : AbstractValidator<NotifyPartnerRegisteredCommand>
    {
        public NotifyPartnerRegisteredCommandValidator()
        {
            RuleFor(x => x.PartnerId)
                .NotEmpty()
                .WithMessage("Partner ID is required");
        }
    }
}
