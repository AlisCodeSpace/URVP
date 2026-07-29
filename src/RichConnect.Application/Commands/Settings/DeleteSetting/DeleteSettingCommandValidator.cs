using FluentValidation;

namespace RICHConnect.Backend.Application.Commands.Settings.DeleteSetting
{
    /// <summary>
    /// Validator for DeleteSettingCommand.
    /// </summary>
    public class DeleteSettingCommandValidator : AbstractValidator<DeleteSettingCommand>
    {
        public DeleteSettingCommandValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Key is required.");
        }
    }
}
