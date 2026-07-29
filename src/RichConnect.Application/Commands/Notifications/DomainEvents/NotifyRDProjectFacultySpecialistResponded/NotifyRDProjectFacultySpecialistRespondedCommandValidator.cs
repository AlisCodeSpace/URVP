using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectFacultySpecialistResponded
{
    public class NotifyRDProjectFacultySpecialistRespondedCommandValidator : AbstractValidator<NotifyRDProjectFacultySpecialistRespondedCommand>
    {
        public NotifyRDProjectFacultySpecialistRespondedCommandValidator()
        {
            RuleFor(x => x.InviteId)
                .NotEmpty().WithMessage("Invite ID is required");

            RuleFor(x => x.RDProjectId)
                .NotEmpty().WithMessage("R&D project ID is required");

            RuleFor(x => x.FacultySpecialistUserId)
                .NotEmpty().WithMessage("facultySpecialist user ID is required");

            RuleFor(x => x.FacultySpecialistName)
                .NotEmpty().WithMessage("facultySpecialist name is required")
                .MaximumLength(200).WithMessage("facultySpecialist name must not exceed 200 characters");

            RuleFor(x => x.ResponseText)
                .NotEmpty().WithMessage("Response text is required");
        }
    }
}
