using FluentValidation;

namespace RICHConnect.Backend.Application.DTOs.Themes
{
    public class RejectResearchFieldDtoValidator : AbstractValidator<RejectResearchFieldDto>
    {
        public RejectResearchFieldDtoValidator()
        {
            RuleFor(dto => dto.RejectionReason)
                .NotEmpty().WithMessage("Rejection reason is required.")
                .MaximumLength(1000).WithMessage("Rejection reason cannot exceed 1000 characters.")
                .MinimumLength(10).WithMessage("Rejection reason must be at least 10 characters to provide meaningful feedback.");
        }
    }
}
