namespace FEA.URVP.Application.DTOs.Email;

/// <summary>
/// Admin-facing SMTP settings. The password itself is never included.
/// </summary>
public sealed class EmailSettingsDto
{
    public bool Enabled { get; init; }

    public string From { get; init; } = string.Empty;

    public string FromName { get; init; } = string.Empty;

    public string? SmtpHost { get; init; }

    public int SmtpPort { get; init; }

    public bool EnableSsl { get; init; }

    public string? UserName { get; init; }

    public bool PasswordIsSet { get; init; }
}
