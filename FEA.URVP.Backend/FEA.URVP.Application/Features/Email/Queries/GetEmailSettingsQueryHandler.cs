using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Email;
using FEA.URVP.Application.Options;
using MediatR;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Application.Queries.Email;

public sealed class GetEmailSettingsQueryHandler
    : IRequestHandler<GetEmailSettingsQuery, EmailSettingsDto>
{
    private readonly IEmailSettingsRepository _settings;
    private readonly EmailOptions _email;

    public GetEmailSettingsQueryHandler(
        IEmailSettingsRepository settings,
        IOptions<EmailOptions> email)
    {
        _settings = settings;
        _email = email.Value;
    }

    public async Task<EmailSettingsDto> Handle(
        GetEmailSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var stored = await _settings.GetAsync(cancellationToken);

        return new EmailSettingsDto
        {
            Enabled = _email.Enabled,
            From = _email.From,
            FromName = _email.FromName,
            SmtpHost = _email.Smtp.Host,
            SmtpPort = _email.Smtp.Port,
            EnableSsl = _email.Smtp.EnableSsl,
            UserName = string.IsNullOrWhiteSpace(_email.Smtp.UserName)
                ? null
                : _email.Smtp.UserName.Trim(),
            PasswordIsSet = stored?.HasPassword == true,
        };
    }
}
