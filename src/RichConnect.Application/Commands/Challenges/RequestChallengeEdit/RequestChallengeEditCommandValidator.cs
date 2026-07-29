using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.RequestChallengeEdit
{
    /// <summary>
    /// Validator for RequestChallengeEditCommand
    /// </summary>
    public class RequestChallengeEditCommandValidator : AbstractValidator<RequestChallengeEditCommand>
    {
        public RequestChallengeEditCommandValidator()
        {
            RuleFor(x => x.ChallengeId)
                .NotEmpty()
                .WithMessage("Challenge ID is required");

            RuleFor(x => x.EditReason)
                .NotEmpty()
                .WithMessage("Edit reason is required")
                .MaximumLength(1000)
                .WithMessage("Edit reason cannot exceed 1000 characters")
                .MinimumLength(10)
                .WithMessage("Edit reason must be at least 10 characters");

            RuleFor(x => x.RequestedBy)
                .NotEmpty()
                .WithMessage("Requested by user ID is required");
        }
    }
}
