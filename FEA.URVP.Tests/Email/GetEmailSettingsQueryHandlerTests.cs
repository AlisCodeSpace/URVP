using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Options;
using FEA.URVP.Application.Queries.Email;
using FEA.URVP.Domain.Entities.Email;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FEA.URVP.Tests.Email;

public sealed class GetEmailSettingsQueryHandlerTests
{
    [Fact]
    public async Task Maps_smtp_configuration_without_exposing_a_password()
    {
        var repo = Substitute.For<IEmailSettingsRepository>();
        repo.GetAsync(Arg.Any<CancellationToken>()).Returns((EmailSettings?)null);

        var handler = new GetEmailSettingsQueryHandler(repo, Options.Create(SampleOptions()));
        var dto = await handler.Handle(new GetEmailSettingsQuery(), CancellationToken.None);

        Assert.True(dto.Enabled);
        Assert.Equal("urvp-system@aub.edu.lb", dto.From);
        Assert.Equal("localhost", dto.SmtpHost);
        Assert.Equal(25, dto.SmtpPort);
        Assert.False(dto.EnableSsl);
        Assert.False(dto.PasswordIsSet);
        Assert.Null(dto.UserName);
        Assert.Null(dto.GetType().GetProperty("Password"));
    }

    [Fact]
    public async Task Reports_a_stored_password_without_returning_it()
    {
        var repo = Substitute.For<IEmailSettingsRepository>();
        repo.GetAsync(Arg.Any<CancellationToken>()).Returns(new EmailSettings
        {
            UserName = "urvp-system@aub.edu.lb",
            ProtectedPassword = "protected-payload",
        });

        var handler = new GetEmailSettingsQueryHandler(repo, Options.Create(SampleOptions()));
        var dto = await handler.Handle(new GetEmailSettingsQuery(), CancellationToken.None);

        Assert.True(dto.PasswordIsSet);
        Assert.Equal("urvp-system@aub.edu.lb", dto.UserName);
        Assert.DoesNotContain("protected-payload", dto.UserName, StringComparison.Ordinal);
    }

    private static EmailOptions SampleOptions() => new()
    {
        Enabled = true,
        From = "urvp-system@aub.edu.lb",
        FromName = "FEA URVP",
        Smtp =
        {
            Host = "localhost",
            Port = 25,
            EnableSsl = false,
        },
    };
}
