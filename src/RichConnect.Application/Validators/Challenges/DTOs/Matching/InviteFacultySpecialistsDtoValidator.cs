using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Matching;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Validator for inviting faculty specialists to challenges
    /// </summary>
    public class InviteFacultySpecialistsDtoValidator : AbstractValidator<InviteFacultySpecialistsDto>
    {
        public InviteFacultySpecialistsDtoValidator()
        {
            RuleFor(x => x.FacultySpecialistIds)
                .NotEmpty().WithMessage("At least one facultySpecialist must be selected")
                .Must(ids => ids.Count >= ChallengeValidationConstants.MIN_FACULTY_SPECIALISTS_PER_INVITE)
                .WithMessage($"At least {ChallengeValidationConstants.MIN_FACULTY_SPECIALISTS_PER_INVITE} facultySpecialist must be selected")
                .Must(ids => ids.Count <= ChallengeValidationConstants.MAX_FACULTY_SPECIALISTS_PER_INVITE)
                .WithMessage($"Cannot invite more than {ChallengeValidationConstants.MAX_FACULTY_SPECIALISTS_PER_INVITE} faculty specialists")
                .Must(ids => ids.All(id => id != Guid.Empty))
                .WithMessage("All facultySpecialist IDs must be valid")
                .Must(ids => ids.Count == ids.Distinct().Count())
                .WithMessage("Duplicate facultySpecialist IDs are not allowed");
        }
    }
}
