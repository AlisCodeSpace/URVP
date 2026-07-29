using FluentValidation;

namespace RICHConnect.Backend.Application.Validators.Configuration;

public class EmailSettings
{
    // Legacy SMTP settings (kept for backward compatibility)
    public string? SmtpServer { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public int? SmtpPort { get; set; }
    public bool? EnableSsl { get; set; }
}

public class SMTP
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool Send { get; set; }
    public string From { get; set; } = string.Empty;
    public bool? EnableSsl { get; set; }
}

public class AzureAdSettings
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string CallbackPath { get; set; } = string.Empty;
}

// Phase 6: AzureStorageSettings removed - database-only file storage (no Azure Blob Storage)
// Legacy AzureStorageSettings class and validator removed as part of Phase 6 cleanup

public class FileStorageSettings
{
    // Phase 6: Database-only storage - feature flags removed (EnableDbBackedStorage, EnableDualWrite, ReadFromDb)
    public long MaxImageSizeBytes { get; set; } = 2097152; // 2 MB
    public long MaxPdfSizeBytes { get; set; } = 10485760; // 10 MB
}

public class EmailSettingsValidator : AbstractValidator<EmailSettings>
{
    public EmailSettingsValidator()
    {
        // Validate SMTP settings
        RuleFor(x => x.SmtpServer)
            .NotEmpty()
            .WithMessage("SMTP Server is required");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("SMTP Username is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("SMTP Password is required");

        RuleFor(x => x.FromEmail)
            .NotEmpty()
            .WithMessage("From Email is required")
            .EmailAddress()
            .WithMessage("From Email must be a valid email address");

        RuleFor(x => x.SmtpPort)
            .NotNull()
            .WithMessage("SMTP Port is required")
            .GreaterThan(0)
            .WithMessage("SMTP Port must be greater than 0");
    }
}

public class AzureAdSettingsValidator : AbstractValidator<AzureAdSettings>
{
    public AzureAdSettingsValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty()
            .WithMessage("Azure AD Tenant ID is required");

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Azure AD Client ID is required");

        // ClientSecret is optional when using id_token-only flow with form_post
        // RuleFor(x => x.ClientSecret)
        //     .NotEmpty()
        //     .WithMessage("Azure AD Client Secret is required");

        RuleFor(x => x.Authority)
            .NotEmpty()
            .WithMessage("Azure AD Authority is required")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Azure AD Authority must be a valid URL");
    }
}

// Phase 6: AzureStorageSettingsValidator removed - database-only file storage

public class FileStorageSettingsValidator : AbstractValidator<FileStorageSettings>
{
    public FileStorageSettingsValidator()
    {
        RuleFor(x => x.MaxImageSizeBytes)
            .GreaterThan(0)
            .WithMessage("Max image size must be greater than 0")
            .LessThanOrEqualTo(2097152)
            .WithMessage("Max image size should not exceed 2 MB (2,097,152 bytes)");

        RuleFor(x => x.MaxPdfSizeBytes)
            .GreaterThan(0)
            .WithMessage("Max PDF size must be greater than 0")
            .LessThanOrEqualTo(10485760)
            .WithMessage("Max PDF size should not exceed 10 MB (10,485,760 bytes)");
    }
}
