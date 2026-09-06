namespace FEA.URVP.Application.Abstractions.Notifications;

public interface IEmailService
{
    bool IsEmailConfigured();

    Task<bool> SendEmailAsync(
        string to,
        string name,
        string subject,
        string body,
        string? actionUrl = null,
        string? actionText = null,
        CancellationToken cancellationToken = default);
}
