using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.UpdateField
{
    public class UpdateFieldCommandValidator : AbstractValidator<UpdateFieldCommand>
    {
        private readonly IResearchFieldRepository _repository;

        public UpdateFieldCommandValidator(IResearchFieldRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            RuleFor(cmd => cmd.FieldId)
                .NotEqual(Guid.Empty).WithMessage("A valid field ID is required.")
                .MustAsync(async (id, cancellation) => await _repository.ExistsAsync(id))
                .WithMessage("The specified research field does not exist.");

            RuleFor(cmd => cmd.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

            RuleFor(cmd => cmd.UpdatedBy)
                .NotEqual(Guid.Empty).WithMessage("A valid updater ID is required.");

            RuleFor(cmd => cmd.Category)
                .MaximumLength(128).WithMessage("Category cannot exceed 128 characters.")
                .When(cmd => !string.IsNullOrEmpty(cmd.Category));

            RuleFor(cmd => cmd.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be a non-negative number.");
        }
    }
}

