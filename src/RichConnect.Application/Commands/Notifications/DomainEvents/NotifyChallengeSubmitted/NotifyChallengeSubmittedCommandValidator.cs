using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyChallengeSubmitted
{
    public class NotifyChallengeSubmittedCommandValidator : AbstractValidator<NotifyChallengeSubmittedCommand>
    {
        public NotifyChallengeSubmittedCommandValidator()
        {
            RuleFor(x => x.ChallengeId)
                .NotEmpty()
                .WithMessage("Challenge ID is required");

            RuleFor(x => x.SubmittedByUserId)
                .NotEmpty()
                .WithMessage("Submitted by user ID is required");
        }
    }
}
