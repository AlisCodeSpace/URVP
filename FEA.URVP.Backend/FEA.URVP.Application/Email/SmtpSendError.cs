namespace FEA.URVP.Application.Email;

/// <summary>
/// Formats SMTP failures for EmailLog so the admin page shows the host and the
/// inner socket/AUTH message, not a generic placeholder.
/// </summary>
public static class SmtpSendError
{
    public static string Format(string? host, int port, Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var label = string.IsNullOrWhiteSpace(host) ? "SMTP" : $"{host}:{port}";
        var messages = new List<string>();
        for (var e = ex; e is not null; e = e.InnerException)
        {
            var message = e.Message?.Trim();
            if (!string.IsNullOrEmpty(message)
                && !messages.Contains(message, StringComparer.Ordinal))
            {
                messages.Add(message);
            }
        }

        var detail = messages.Count > 0 ? string.Join(" → ", messages) : ex.GetType().Name;
        return $"{label}: {detail}";
    }

    public static string? Join(params string?[] errors)
    {
        var parts = errors.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
        return parts.Length == 0 ? null : string.Join(" | ", parts);
    }
}
