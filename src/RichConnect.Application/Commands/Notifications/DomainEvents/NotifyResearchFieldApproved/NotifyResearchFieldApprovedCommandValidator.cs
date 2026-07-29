using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldApproved
{
    public class NotifyResearchFieldApprovedCommandValidator : AbstractValidator<NotifyResearchFieldApprovedCommand>
    {
        public NotifyResearchFieldApprovedCommandValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty()
                .WithMessage("Field ID is required");

            RuleFor(x => x.ApprovedByUserId)
                .NotEmpty()
                .WithMessage("Approved by user ID is required");
        }
    }
}
