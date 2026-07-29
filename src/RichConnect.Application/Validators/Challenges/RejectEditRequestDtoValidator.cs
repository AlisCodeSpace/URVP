using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Challenge;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Validator for RejectEditRequestDto
    /// </summary>
    public class RejectEditRequestDtoValidator : AbstractValidator<RejectEditRequestDto>
    {
        public RejectEditRequestDtoValidator()
        {
            RuleFor(x => x.AdminResponse)
                .NotEmpty()
                .WithMessage("Admin response is required when rejecting an edit request")
                .MinimumLength(10)
                .WithMessage("Admin response must be at least 10 characters long")
                .MaximumLength(1000)
                .WithMessage("Admin response must not exceed 1000 characters");
        }
    }
}
