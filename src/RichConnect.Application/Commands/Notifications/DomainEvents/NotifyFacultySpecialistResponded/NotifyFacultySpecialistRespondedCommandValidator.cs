using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyFacultySpecialistResponded
{
    public class NotifyFacultySpecialistRespondedCommandValidator : AbstractValidator<NotifyFacultySpecialistRespondedCommand>
    {
        public NotifyFacultySpecialistRespondedCommandValidator()
        {
            RuleFor(x => x.InviteId)
                .NotEmpty()
                .WithMessage("Invite ID is required.");

            RuleFor(x => x.ChallengeId)
                .NotEmpty()
                .WithMessage("Challenge ID is required.");

            RuleFor(x => x.FacultySpecialistUserId)
                .NotEmpty()
                .WithMessage("facultySpecialist user ID is required.");

            RuleFor(x => x.FacultySpecialistName)
                .NotEmpty()
                .WithMessage("facultySpecialist name is required.")
                .MaximumLength(200)
                .WithMessage("facultySpecialist name must not exceed 200 characters.");

            RuleFor(x => x.ResponseText)
                .NotEmpty()
                .WithMessage("Response text is required.")
                .MaximumLength(50)
                .WithMessage("Response text must not exceed 50 characters.");
        }
    }
}
