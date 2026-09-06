namespace FEA.URVP.Application.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }

    public string From { get; set; } = "noreply@mail.aub.edu";

    public string FromName { get; set; } = "FEA Undergraduate Research Volunteer Program";

    public string? PortalSignInUrl { get; set; }

    public string SignInActionText { get; set; } = "Sign in to URVP";

    public SmtpServerOptions Smtp { get; set; } = new();

    public SmtpServerOptions? SmtpFallback { get; set; }
}

public sealed class SmtpServerOptions
{
    public string? Host { get; set; }

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);
}
