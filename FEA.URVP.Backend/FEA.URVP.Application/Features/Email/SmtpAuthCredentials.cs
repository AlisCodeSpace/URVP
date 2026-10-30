namespace FEA.URVP.Application.Email;

/// <summary>
/// Resolves SMTP AUTH credentials. A stored (admin) password takes precedence
/// over configuration. AUTH is used only when both a username and a password
/// are present. A password without a username is ignored so an unauthenticated
/// campus relay (port 25, no SSL) keeps working.
/// </summary>
public static class SmtpAuthCredentials
{
    public readonly record struct Auth(string UserName, string Password);

    public static Auth? Resolve(
        string? configUserName,
        string? configPassword,
        string? storedPassword)
    {
        var userName = FirstNonEmpty(configUserName);
        var password = FirstNonEmpty(storedPassword, configPassword);

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        return new Auth(userName, password);
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
