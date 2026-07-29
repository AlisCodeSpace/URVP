using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyFacultySpecialistInvited
{
    public class NotifyFacultySpecialistInvitedCommandValidator : AbstractValidator<NotifyFacultySpecialistInvitedCommand>
    {
        public NotifyFacultySpecialistInvitedCommandValidator()
        {
            RuleFor(x => x.InviteId)
                .NotEmpty()
                .WithMessage("Invite ID is required");

            RuleFor(x => x.ChallengeId)
                .NotEmpty()
                .WithMessage("Challenge ID is required");

            RuleFor(x => x.FacultySpecialistUserId)
                .NotEmpty()
                .WithMessage("facultySpecialist user ID is required");

            RuleFor(x => x.FacultySpecialistName)
                .NotEmpty()
                .WithMessage("facultySpecialist name is required");

            RuleFor(x => x.ChallengeTitle)
                .NotEmpty()
                .WithMessage("Challenge title is required");
        }
    }
}
