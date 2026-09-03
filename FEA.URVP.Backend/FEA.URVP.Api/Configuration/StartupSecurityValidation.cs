using FEA.URVP.Api.Configuration.Auth;
using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Services;

namespace FEA.URVP.Api.Configuration;

/// <summary>
/// Reports insecure configuration at startup instead of letting it surface as a breach later.
/// </summary>
/// <remarks>
/// Findings are logged rather than thrown so a misconfigured value cannot take a running site
/// down, but they are logged at Error or Warning so they are visible in Seq and in the deployment
/// log. Every item here corresponds to a control that only configuration can complete.
/// </remarks>
public static class StartupSecurityValidation
{
    private const string LoggerCategory = "FEA.URVP.Api.Security.Startup";

    public static void ValidateSecurityConfiguration(this WebApplication app)
    {
        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(LoggerCategory);

        var configuration = app.Configuration;
        var environment = app.Environment;

        CorsConfiguration.ValidateOrigins(configuration, environment, logger);
        ForwardedHeadersConfiguration.ValidateTrustedProxies(configuration, environment, logger);

        LogDevSignInState(configuration, environment, logger);

        if (environment.IsDevelopment())
        {
            LogMissingSsoInDevelopment(configuration, logger);
            return;
        }

        ValidateSsoConfiguration(configuration, environment, logger);
        ValidateAllowedHosts(configuration, logger);
        ValidateConnectionString(configuration, logger, environment);
        ValidateStartupMigrations(configuration, logger);
        ValidateFrontendPresence(app, logger);
    }

    private static void LogDevSignInState(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        var requested = configuration.GetValue(DevSignInPolicy.ConfigurationKey, false);
        var effective = DevSignInPolicy.IsEnabled(configuration, environment);

        if (requested && !effective)
        {
            logger.LogWarning(
                "{Key} is set but demo sign-in is disabled because the environment is {Environment}.",
                DevSignInPolicy.ConfigurationKey,
                environment.EnvironmentName);
            return;
        }

        if (effective)
        {
            logger.LogWarning(
                "Demo email sign-in is ENABLED in {Environment}. Any caller who knows a demo "
                + "address can obtain a session without SSO.",
                environment.EnvironmentName);
        }
    }

    private static void LogMissingSsoInDevelopment(IConfiguration configuration, ILogger logger)
    {
        var missing = AzureAdOidcConfiguration.MissingSettings(configuration);

        if (missing.Count > 0)
        {
            logger.LogWarning(
                "Azure AD sign-in is not registered because {Missing} are unset. SSO routes will "
                + "report 503 and demo sign-in is the only local path.",
                string.Join(", ", missing));
        }
    }

    /// <summary>
    /// Azure AD is the only real sign-in path outside Development, so an incomplete registration
    /// is a failed deployment. This is the one finding that throws: booting anyway produces a site
    /// that nobody can sign in to, and a crash with this message is far easier to diagnose than
    /// the alternative.
    /// </summary>
    private static void ValidateSsoConfiguration(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        var missing = AzureAdOidcConfiguration.MissingSettings(configuration);

        if (missing.Count == 0)
        {
            return;
        }

        var message =
            $"Azure AD authentication is not configured in {environment.EnvironmentName}: "
            + $"{string.Join(", ", missing)} must be supplied through the secret store or "
            + "protected environment variables.";

        logger.LogCritical("{Message}", message);
        throw new InvalidOperationException(message);
    }

    private static void ValidateAllowedHosts(IConfiguration configuration, ILogger logger)
    {
        var allowedHosts = configuration["AllowedHosts"];

        if (string.IsNullOrWhiteSpace(allowedHosts) || allowedHosts.Contains('*'))
        {
            logger.LogError(
                "AllowedHosts is {AllowedHosts}. Set it to the real production hostnames so the "
                + "app rejects Host header spoofing and cache-poisoning attempts.",
                allowedHosts ?? "(unset)");
        }
    }

    private static void ValidateConnectionString(
        IConfiguration configuration,
        ILogger logger,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("SqlServerConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        if (connectionString.Contains("TrustServerCertificate=true", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogError(
                "The SQL connection string sets TrustServerCertificate=true in {Environment}. "
                + "The server certificate is not verified, so the database connection can be "
                + "intercepted. Install a trusted certificate and remove the setting.",
                environment.EnvironmentName);
        }

        var encryptionDisabled =
            connectionString.Contains("Encrypt=false", StringComparison.OrdinalIgnoreCase)
            || connectionString.Contains("Encrypt=no", StringComparison.OrdinalIgnoreCase);

        if (encryptionDisabled)
        {
            logger.LogError(
                "The SQL connection string disables encryption in {Environment}. Set Encrypt=True.",
                environment.EnvironmentName);
        }
        else if (!connectionString.Contains("Encrypt=", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning(
                "The SQL connection string does not set Encrypt explicitly. Microsoft.Data.SqlClient "
                + "defaults to encryption, but set Encrypt=True so the intent survives a driver change.");
        }
    }

    private static void ValidateStartupMigrations(IConfiguration configuration, ILogger logger)
    {
        if (configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
        {
            logger.LogWarning(
                "Database:ApplyMigrationsOnStartup is enabled outside Development. Schema changes "
                + "will run automatically on every start, with no backup or rollback step. Prefer a "
                + "controlled release task.");
        }
    }

    private static void ValidateFrontendPresence(WebApplication app, ILogger logger)
    {
        var frontend = app.Services.GetRequiredService<ExportedFrontendProvider>();

        if (!frontend.IsEnabled)
        {
            logger.LogWarning(
                "No exported frontend was found, so this instance serves API traffic only. The "
                + "same-origin deployment expects the Next.js export to be published into the "
                + "Security:Frontend:RootPath directory.");
        }
    }
}
