using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;

namespace RICHConnect.Backend.Application.Commands.InviteFacultySpecialists
{
    public class InviteFacultySpecialistsCommandValidator : AbstractValidator<InviteFacultySpecialistsCommand>
    {
        private readonly IChallengeRepository _repository;

        public InviteFacultySpecialistsCommandValidator(IChallengeRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.ChallengeId)
                .NotEmpty().WithMessage("Challenge ID is required")
                .MustAsync(async (id, cancellation) => await _repository.ExistsAsync(id))
                .WithMessage("Challenge does not exist");

            RuleFor(x => x.FacultySpecialistIds)
                .NotEmpty().WithMessage("At least one facultySpecialist must be invited")
                .Must(ids => ids.Count > 0).WithMessage("At least one facultySpecialist must be invited");

            RuleFor(x => x.AdminId)
                .NotEmpty().WithMessage("Admin ID is required");
        }
    }
}
