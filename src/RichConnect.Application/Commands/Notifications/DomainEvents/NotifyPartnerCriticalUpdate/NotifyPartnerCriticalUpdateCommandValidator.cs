using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyPartnerCriticalUpdate
{
    public class NotifyPartnerCriticalUpdateCommandValidator : AbstractValidator<NotifyPartnerCriticalUpdateCommand>
    {
        public NotifyPartnerCriticalUpdateCommandValidator()
        {
            RuleFor(x => x.PartnerId)
                .NotEmpty()
                .WithMessage("Partner ID is required");

            RuleFor(x => x.UpdatedByUserId)
                .NotEmpty()
                .WithMessage("Updated by user ID is required");

            RuleFor(x => x.CriticalFieldsChanged)
                .NotEmpty()
                .WithMessage("Critical fields changed list cannot be empty");
        }
    }
}
