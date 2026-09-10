namespace FEA.URVP.Application.Email;

/// <summary>
/// Resolves SMTP AUTH credentials. A stored (admin) password takes precedence
/// over configuration. Username comes from configuration, or the From address
/// when only a password is present. Credentials are omitted when neither a
/// username nor a password is present, so an unauthenticated local relay still
/// works.
/// </summary>
public static class SmtpAuthCredentials
{
    public readonly record struct Auth(string UserName, string Password);

    public static Auth? Resolve(
        string? configUserName,
        string? configPassword,
        string? storedPassword,
        string fromAddress)
    {
        var userName = FirstNonEmpty(configUserName);
        var password = FirstNonEmpty(storedPassword, configPassword);

        if (string.IsNullOrWhiteSpace(userName) && string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = fromAddress.Trim();
        }

        return new Auth(userName, password ?? string.Empty);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }
}
