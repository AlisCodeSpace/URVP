using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Abstractions.Security;
using FEA.URVP.Application.Commands.Base;
using FEA.URVP.Application.DTOs.Email;
using FEA.URVP.Application.Queries.Email;
using MediatR;
using Microsoft.Extensions.Logging;
using EmailSettingsRow = FEA.URVP.Domain.Entities.Email.EmailSettings;

namespace FEA.URVP.Application.Commands.Email.Update;

public sealed class UpdateEmailSettingsCommandHandler
    : BaseCommandHandler<UpdateEmailSettingsCommand, EmailSettingsDto>
{
    private readonly IEmailSettingsRepository _settings;
    private readonly ISmtpCredentialProtector _protector;
    private readonly IMediator _mediator;

    public UpdateEmailSettingsCommandHandler(
        ILogger<UpdateEmailSettingsCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IEmailSettingsRepository settings,
        ISmtpCredentialProtector protector,
        IMediator mediator)
        : base(logger, unitOfWork)
    {
        _settings = settings;
        _protector = protector;
        _mediator = mediator;
    }

    protected override async Task<EmailSettingsDto> HandleInternal(
        UpdateEmailSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var row = await _settings.GetAsync(cancellationToken);

        if (row is null)
        {
            row = new EmailSettingsRow
            {
                Id = EmailSettingsRow.SingletonId,
                CreatedAt = now,
            };
            _settings.Add(row);
        }

        row.UserName = string.IsNullOrWhiteSpace(request.UserName)
            ? null
            : request.UserName.Trim();
        row.UpdatedAt = now;
        row.UpdatedByUserId = request.UpdatedByUserId == Guid.Empty
            ? null
            : request.UpdatedByUserId;

        if (request.ClearPassword)
        {
            row.ProtectedPassword = null;
            Logger.LogInformation("SMTP password cleared by an administrator.");
        }
        else if (!string.IsNullOrWhiteSpace(request.Password))
        {
            row.ProtectedPassword = _protector.Protect(request.Password);
            Logger.LogInformation("SMTP password updated by an administrator.");
        }

        await UnitOfWork.SaveChangesAsync(cancellationToken);

        return await _mediator.Send(new GetEmailSettingsQuery(), cancellationToken);
    }
}
