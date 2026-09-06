using System.Net;
using System.Net.Mail;
using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Options;
using FEA.URVP.Domain.Entities.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Infrastructure.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly IEmailLogRepository _emailLogs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(
        IOptions<EmailOptions> options,
        IEmailLogRepository emailLogs,
        IUnitOfWork unitOfWork,
        ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
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

            success = await TrySendAsync(
                _options.Smtp,
                to,
                name,
                subject,
                html,
                cancellationToken);

            if (!success && _options.SmtpFallback is { IsConfigured: true } fallback)
            {
                _logger.LogWarning("Primary SMTP failed for {To}; trying fallback host", to);
                success = await TrySendAsync(fallback, to, name, subject, html, cancellationToken);
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

    private async Task<bool> TrySendAsync(
        SmtpServerOptions smtp,
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

            if (!string.IsNullOrWhiteSpace(smtp.UserName))
            {
                client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);
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
