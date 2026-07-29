using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Notifications.NotifyRDProjectFacultySpecialistInvited
{
    public class NotifyRDProjectFacultySpecialistInvitedCommandValidator : AbstractValidator<NotifyRDProjectFacultySpecialistInvitedCommand>
    {
        public NotifyRDProjectFacultySpecialistInvitedCommandValidator()
        {
            RuleFor(x => x.InviteId)
                .NotEmpty().WithMessage("Invite ID is required");

            RuleFor(x => x.RDProjectId)
                .NotEmpty().WithMessage("R&D project ID is required");

            RuleFor(x => x.FacultySpecialistUserId)
                .NotEmpty().WithMessage("facultySpecialist user ID is required");

            RuleFor(x => x.ProjectTitle)
                .NotEmpty().WithMessage("Project title is required");
        }
    }
}
