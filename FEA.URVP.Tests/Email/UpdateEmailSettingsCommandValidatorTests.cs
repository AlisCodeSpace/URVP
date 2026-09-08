using FEA.URVP.Application.Commands.Email.Update;
using FEA.URVP.Application.Validators.Email;
using FEA.URVP.Domain.Entities.Email;

namespace FEA.URVP.Tests.Email;

public sealed class UpdateEmailSettingsCommandValidatorTests
{
    [Fact]
    public async Task Rejects_clearing_and_setting_a_password_together()
    {
        var result = await new UpdateEmailSettingsCommandValidator().ValidateAsync(
            new UpdateEmailSettingsCommand
            {
                Password = "mailbox-secret",
                ClearPassword = true,
            });

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            e => e.ErrorMessage.Contains("Clear the stored password", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Rejects_an_oversized_password()
    {
        var result = await new UpdateEmailSettingsCommandValidator().ValidateAsync(
            new UpdateEmailSettingsCommand
            {
                Password = new string('x', EmailSettings.PasswordPlaintextMaxLength + 1),
            });

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Accepts_a_password_update()
    {
        var result = await new UpdateEmailSettingsCommandValidator().ValidateAsync(
            new UpdateEmailSettingsCommand
            {
                UserName = "urvp-system@aub.edu.lb",
                Password = "mailbox-secret",
            });

        Assert.True(result.IsValid);
    }
}
