using FluentValidation;

namespace RICHConnect.Backend.Application.DTOs.Themes
{
    public class UpdateResearchFieldDtoValidator : AbstractValidator<UpdateResearchFieldDto>
    {
        public UpdateResearchFieldDtoValidator()
        {
            RuleFor(dto => dto.Name)
                .NotEmpty().WithMessage("Research field name is required.")
                .MaximumLength(200).WithMessage("Research field name cannot exceed 200 characters.")
                .MinimumLength(3).WithMessage("Research field name must be at least 3 characters.");

            RuleFor(dto => dto.Category)
                .MaximumLength(128).WithMessage("Category cannot exceed 128 characters.")
                .When(dto => !string.IsNullOrEmpty(dto.Category));

            RuleFor(dto => dto.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be a non-negative number.");
        }
    }
}
