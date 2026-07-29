using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Validator for ApproveEditRequestDto
    /// </summary>
    public class ApproveEditRequestDtoValidator : AbstractValidator<ApproveEditRequestDto>
    {
        public ApproveEditRequestDtoValidator()
        {
            RuleFor(x => x.AdminResponse)
                .MaximumLength(1000)
                .WithMessage("Admin response must not exceed 1000 characters");
        }
    }
}
