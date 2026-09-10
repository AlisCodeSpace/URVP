using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Abstractions.Security;
using FEA.URVP.Application.Commands.Email.Update;
using FEA.URVP.Application.DTOs.Email;
using FEA.URVP.Application.Queries.Email;
using FEA.URVP.Domain.Entities.Email;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FEA.URVP.Tests.Email;

public sealed class UpdateEmailSettingsCommandHandlerTests
{
    [Fact]
    public async Task Stores_an_encrypted_password_never_plaintext()
    {
        var repo = Substitute.For<IEmailSettingsRepository>();
        repo.GetAsync(Arg.Any<CancellationToken>()).Returns((EmailSettings?)null);

        EmailSettings? added = null;
        repo.When(x => x.Add(Arg.Any<EmailSettings>())).Do(call => added = call.Arg<EmailSettings>());

        var protector = Substitute.For<ISmtpCredentialProtector>();
        protector.Protect("mailbox-secret").Returns("protected-payload");

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetEmailSettingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new EmailSettingsDto { PasswordIsSet = true });

        var handler = new UpdateEmailSettingsCommandHandler(
            NullLogger<UpdateEmailSettingsCommandHandler>.Instance,
            unitOfWork,
            repo,
            protector,
            mediator);

        var dto = await handler.Handle(
            new UpdateEmailSettingsCommand
            {
                UpdatedByUserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Password = "mailbox-secret",
            },
            CancellationToken.None);

        Assert.NotNull(added);
        Assert.Equal("protected-payload", added!.ProtectedPassword);
        Assert.NotEqual("mailbox-secret", added.ProtectedPassword);
        Assert.Null(added.UserName);
        Assert.True(dto.PasswordIsSet);
        protector.Received(1).Protect("mailbox-secret");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Blank_password_leaves_the_stored_secret_unchanged()
    {
        var existing = new EmailSettings
        {
            ProtectedPassword = "existing-payload",
            UserName = "old-user",
        };
        var repo = Substitute.For<IEmailSettingsRepository>();
        repo.GetAsync(Arg.Any<CancellationToken>()).Returns(existing);

        var protector = Substitute.For<ISmtpCredentialProtector>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetEmailSettingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new EmailSettingsDto { PasswordIsSet = true });

        var handler = new UpdateEmailSettingsCommandHandler(
            NullLogger<UpdateEmailSettingsCommandHandler>.Instance,
            unitOfWork,
            repo,
            protector,
            mediator);

        await handler.Handle(
            new UpdateEmailSettingsCommand
            {
                Password = "   ",
            },
            CancellationToken.None);

        Assert.Equal("existing-payload", existing.ProtectedPassword);
        Assert.Null(existing.UserName);
        protector.DidNotReceive().Protect(Arg.Any<string>());
    }

    [Fact]
    public async Task Clear_password_removes_the_stored_secret()
    {
        var existing = new EmailSettings { ProtectedPassword = "existing-payload" };
        var repo = Substitute.For<IEmailSettingsRepository>();
        repo.GetAsync(Arg.Any<CancellationToken>()).Returns(existing);

        var protector = Substitute.For<ISmtpCredentialProtector>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var mediator = Substitute.For<IMediator>();
        mediator
            .Send(Arg.Any<GetEmailSettingsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new EmailSettingsDto { PasswordIsSet = false });

        var handler = new UpdateEmailSettingsCommandHandler(
            NullLogger<UpdateEmailSettingsCommandHandler>.Instance,
            unitOfWork,
            repo,
            protector,
            mediator);

        var dto = await handler.Handle(
            new UpdateEmailSettingsCommand { ClearPassword = true },
            CancellationToken.None);

        Assert.Null(existing.ProtectedPassword);
        Assert.False(dto.PasswordIsSet);
        protector.DidNotReceive().Protect(Arg.Any<string>());
    }
}
