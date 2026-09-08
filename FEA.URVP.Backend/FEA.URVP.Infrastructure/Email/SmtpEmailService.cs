using System.Net;
using System.Net.Mail;
using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Abstractions.Security;
using FEA.URVP.Application.Email;
using FEA.URVP.Application.Options;
using FEA.URVP.Domain.Entities.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Infrastructure.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly IEmailSettingsRepository _emailSettings;
    private readonly ISmtpCredentialProtector _protector;
    private readonly IEmailLogRepository _emailLogs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<EmailOptions> options,
        IEmailSettingsRepository emailSettings,
        ISmtpCredentialProtector protector,
        IEmailLogRepository emailLogs,
        IUnitOfWork unitOfWork,
        ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _emailSettings = emailSettings;
        _protector = protector;
        _emailLogs = emailLogs;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public bool IsEmailConfigured()
    {
        var configured = _options.Enabled && _options.Smtp.IsConfigured;
        if (!configured)
        {
            _logger.LogInformation("Email sending is disabled or SMTP is not configured.");
        }

        return configured;
    }

    public async Task<bool> SendEmailAsync(
        string to,
        string name,
        string subject,
        string body,
        string? actionUrl = null,
        string? actionText = null,
        CancellationToken cancellationToken = default)
    {
        var html = EmailHtmlTemplate.LooksLikeHtml(body)
            ? body
            : EmailHtmlTemplate.Wrap(subject, body, actionUrl, actionText);

        string? exception = null;
        var success = false;

        try
        {
            if (!IsEmailConfigured())
            {
                exception = "Email sending is disabled or SMTP is not configured.";
                return false;
            }

            var auth = await ResolveAuthAsync(cancellationToken);

            success = await TrySendAsync(
                _options.Smtp,
                auth,
                to,
                name,
                subject,
                html,
                cancellationToken);

            if (!success && _options.SmtpFallback is { IsConfigured: true } fallback)
            {
                _logger.LogWarning("Primary SMTP failed for {To}; trying fallback host", to);
                success = await TrySendAsync(fallback, auth, to, name, subject, html, cancellationToken);
            }

            if (!success)
            {
                exception ??= "SMTP send failed on primary and fallback.";
            }

            return success;
        }
        catch (Exception ex)
        {
            exception = ex.ToString();
            _logger.LogError(ex, "SMTP send failed for {To}", to);
            return false;
        }
        finally
        {
            await WriteLogAsync(to, html, success, exception, cancellationToken);
        }
    }

    private async Task<SmtpAuthCredentials.Auth?> ResolveAuthAsync(CancellationToken cancellationToken)
    {
        string? storedUserName = null;
        string? storedPassword = null;

        try
        {
            var stored = await _emailSettings.GetAsync(cancellationToken);
            storedUserName = stored?.UserName;
            if (stored?.ProtectedPassword is { Length: > 0 } payload
                && _protector.TryUnprotect(payload, out var plaintext))
            {
                storedPassword = plaintext;
            }
            else if (stored?.HasPassword == true)
            {
                _logger.LogError(
                    "Stored SMTP password could not be decrypted. Re-enter it on the admin Email page.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load stored SMTP credentials; falling back to configuration.");
        }

        return SmtpAuthCredentials.Resolve(
            _options.Smtp.UserName,
            _options.Smtp.Password,
            storedUserName,
            storedPassword,
            _options.From);
    }

    private async Task<bool> TrySendAsync(
        SmtpServerOptions smtp,
        SmtpAuthCredentials.Auth? auth,
        string to,
        string name,
        string subject,
        string html,
        CancellationToken cancellationToken)
    {
        try
        {
#pragma warning disable SYSLIB0014
            using var client = new SmtpClient(smtp.Host, smtp.Port)
            {
                EnableSsl = smtp.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
#pragma warning restore SYSLIB0014

            if (auth is { } credentials)
            {
                client.Credentials = new NetworkCredential(credentials.UserName, credentials.Password);
            }

            using var message = new MailMessage
            {
                From = new MailAddress(_options.From, _options.FromName),
                Subject = subject,
                Body = html,
                IsBodyHtml = true,
            };
            message.To.Add(new MailAddress(to, string.IsNullOrWhiteSpace(name) ? to : name));

            await client.SendMailAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP host {Host} failed for {To}", smtp.Host, to);
            return false;
        }
    }

    private async Task WriteLogAsync(
        string to,
        string body,
        bool success,
        string? exception,
        CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            _emailLogs.Add(new EmailLog
            {
                From = _options.From,
                To = to,
                Body = body,
                Exception = Truncate(exception, EmailLog.ExceptionMaxLength),
                Success = success,
                CreatedOn = now,
                ModifiedOn = now,
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write EmailLog for {To}", to);
        }
    }

    private static string? Truncate(string? value, int maxLength) =>
        value is null || value.Length <= maxLength ? value : value[..maxLength];
}
