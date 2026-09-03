namespace FEA.URVP.Api.Configuration.Auth;

/// <summary>
/// Availability of the demo email sign-in path.
/// </summary>
/// <remarks>
/// Demo sign-in mints a fully privileged session from nothing but an email address, bypassing
/// Azure AD entirely. It is therefore unconditionally unavailable in Production: no configuration
/// value, environment variable or secret can re-enable it there. Outside Production it defaults
/// to on only in Development and must be opted into explicitly anywhere else.
/// </remarks>
public static class DevSignInPolicy
{
    public const string ConfigurationKey = "Auth:EnableDevSignIn";

    public static bool IsEnabled(IConfiguration configuration, IHostEnvironment environment) =>
        !environment.IsProduction()
        && configuration.GetValue(ConfigurationKey, environment.IsDevelopment());
}
