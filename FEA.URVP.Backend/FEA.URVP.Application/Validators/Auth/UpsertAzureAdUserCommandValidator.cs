using FEA.URVP.Application.Commands.Auth.AzureAd;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Auth;

public sealed class UpsertAzureAdUserCommandValidator : AbstractValidator<UpsertAzureAdUserCommand>
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

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required.")
            .MaximumLength(64).WithMessage("User name cannot exceed 64 characters.");

        RuleFor(x => x.Affiliation)
            .NotEmpty().WithMessage("Affiliation is required.")
            .MaximumLength(256).WithMessage("Affiliation cannot exceed 256 characters.");

        RuleFor(x => x.ProfileImageUrl)
            .MaximumLength(512).WithMessage("Profile image URL cannot exceed 512 characters.")
            .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl));
    }
}
