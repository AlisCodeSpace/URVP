using FluentValidation;
using RICHConnect.Backend.Application.Commands.Auth.AzureAd;

namespace RICHConnect.Backend.Application.Validators.Auth
{
    /// <summary>
    /// Validator for UpsertAzureAdUserCommand
    /// </summary>
    public class UpsertAzureAdUserCommandValidator : AbstractValidator<UpsertAzureAdUserCommand>
    {
        public UpsertAzureAdUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.")
                .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(128).WithMessage("Name cannot exceed 128 characters.");

            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(512).WithMessage("Profile image URL cannot exceed 512 characters.")
                .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl));
        }
    }
}
