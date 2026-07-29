using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyChallengeApproved
{
    public class NotifyChallengeApprovedCommandValidator : AbstractValidator<NotifyChallengeApprovedCommand>
    {
        public NotifyChallengeApprovedCommandValidator()
        {
            RuleFor(x => x.ChallengeId)
                .NotEmpty()
                .WithMessage("Challenge ID is required.");

            RuleFor(x => x.ApprovedByUserId)
                .NotEmpty()
                .WithMessage("Approved by user ID is required.");
        }
    }
}
