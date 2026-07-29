using FluentValidation;
using RICHConnect.Backend.Application.Commands.Auth.AzureB2C;

namespace RICHConnect.Backend.Application.Validators.Auth
{
    /// <summary>
    /// Validator for UpsertAzureB2CUserCommand
    /// </summary>
    public class UpsertAzureB2CUserCommandValidator : AbstractValidator<UpsertAzureB2CUserCommand>
    {
        public UpsertAzureB2CUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email must be a valid email address")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(128).WithMessage("Name must not exceed 128 characters");
        }
    }
}
