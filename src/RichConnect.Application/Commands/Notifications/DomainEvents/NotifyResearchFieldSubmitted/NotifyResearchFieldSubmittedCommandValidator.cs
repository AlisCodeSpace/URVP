using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyResearchFieldSubmitted
{
    public class NotifyResearchFieldSubmittedCommandValidator : AbstractValidator<NotifyResearchFieldSubmittedCommand>
    {
        public NotifyResearchFieldSubmittedCommandValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty()
                .WithMessage("Field ID is required");

            RuleFor(x => x.SubmittedByUserId)
                .NotEmpty()
                .WithMessage("Submitted by user ID is required");
        }
    }
}
