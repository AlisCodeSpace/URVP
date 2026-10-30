using FEA.URVP.Application.Features.Email.Commands.Update;
using FluentValidation;
using EmailSettingsRow = FEA.URVP.Domain.Entities.Email.EmailSettings;

namespace FEA.URVP.Application.Features.Email.Validators;

public sealed class UpdateEmailSettingsCommandValidator
    : AbstractValidator<UpdateEmailSettingsCommand>
{
    public UpdateEmailSettingsCommandValidator()
    {
        RuleFor(x => x.Password)
            .MaximumLength(EmailSettingsRow.PasswordPlaintextMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.Password));

        RuleFor(x => x)
            .Must(x => !x.ClearPassword || string.IsNullOrWhiteSpace(x.Password))
            .WithMessage("Clear the stored password or set a new one, not both.");
    }
}
