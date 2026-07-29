using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.CreateField
{
    public class CreateFieldCommandValidator : AbstractValidator<CreateFieldCommand>
    {

        public CreateFieldCommandValidator()
        {
            RuleFor(cmd => cmd.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

            RuleFor(cmd => cmd.SubmittedBy)
                .NotEqual(Guid.Empty).WithMessage("A valid submitter ID is required.");

            RuleFor(cmd => cmd.Category)
                .MaximumLength(128).WithMessage("Category cannot exceed 128 characters.")
                .When(cmd => !string.IsNullOrEmpty(cmd.Category));

            RuleFor(cmd => cmd.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be a non-negative number.");
        }
    }
}

