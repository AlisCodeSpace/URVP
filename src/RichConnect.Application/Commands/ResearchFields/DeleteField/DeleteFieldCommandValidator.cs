using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.DeleteField
{
    public class DeleteFieldCommandValidator : AbstractValidator<DeleteFieldCommand>
    {
        private readonly IResearchFieldRepository _repository;

        public DeleteFieldCommandValidator(IResearchFieldRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            RuleFor(cmd => cmd.FieldId)
                .NotEqual(Guid.Empty)
                .WithMessage("A valid research field ID is required.")
                .MustAsync(async (id, cancellation) => await _repository.ExistsAsync(id))
                .WithMessage("The specified research field does not exist.");

            RuleFor(cmd => cmd.DeletedBy)
                .NotEqual(Guid.Empty)
                .WithMessage("A valid deleter ID is required.");
                
            // Note: In a real-world scenario, we would check for dependencies here
            // For example, check if there are any challenges using this field
            // This would be implemented once the Challenge-ResearchField relationship is established
        }
    }
}

