using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Settings.SetSetting
{
    /// <summary>
    /// Validator for SetSettingCommand. Enforces key format (Section.SubKey style, max 256 chars, no dangerous chars),
    /// value/category/description lengths. Optional: add an allow-list of keys in this validator to restrict which keys can be created/updated via API.
    /// </summary>
    public class SetSettingCommandValidator : AbstractValidator<SetSettingCommand>
    {
        private const int KeyMaxLength = 256;
        private const int ValueMaxLength = 4096;
        private const int CategoryMaxLength = 128;
        private const int DescriptionMaxLength = 512;

        public SetSettingCommandValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Key is required.")
                .MaximumLength(KeyMaxLength).WithMessage($"Key cannot exceed {KeyMaxLength} characters.")
                .Matches(@"^[a-zA-Z0-9._\-]+$").WithMessage("Key may only contain letters, digits, dots, underscores, and hyphens (e.g. Section.SubKey).");

            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Value is required.")
                .MaximumLength(ValueMaxLength).WithMessage($"Value cannot exceed {ValueMaxLength} characters.");

            RuleFor(x => x.Category)
                .MaximumLength(CategoryMaxLength).WithMessage($"Category cannot exceed {CategoryMaxLength} characters.")
                .When(x => !string.IsNullOrEmpty(x.Category));

            RuleFor(x => x.Description)
                .MaximumLength(DescriptionMaxLength).WithMessage($"Description cannot exceed {DescriptionMaxLength} characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.UpdatedBy)
                .NotEmpty().WithMessage("UpdatedBy (admin user ID) is required.");
        }
    }
}
