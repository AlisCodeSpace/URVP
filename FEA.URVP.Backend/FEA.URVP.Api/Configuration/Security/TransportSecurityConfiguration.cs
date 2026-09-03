namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// HSTS and HTTPS redirection.
/// </summary>
public static class TransportSecurityConfiguration
{
    private const int MinimumMaxAgeDays = 365;

    public static IServiceCollection AddHstsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var hsts = configuration
            .GetSection(SecurityOptions.SectionName)
            .Get<SecurityOptions>()?.Hsts
            ?? new HstsOptions();

        services.AddHsts(options =>
        {
            // A max-age below a year lets a downgrade attack succeed for anyone whose pin has
            // lapsed, so the configured value is treated as a floor rather than an override.
            var days = Math.Max(hsts.MaxAgeDays, MinimumMaxAgeDays);
            options.MaxAge = TimeSpan.FromDays(days);

            // Both stay opt-in: IncludeSubDomains breaks any sibling host still on HTTP, and
            // Preload is effectively irreversible once the domain is accepted by browsers.
            options.IncludeSubDomains = hsts.IncludeSubDomains;
            options.Preload = hsts.Preload;
        });

        return services;
    }

    /// <summary>
    /// Whether to enforce HTTPS in-process.
    /// </summary>
    /// <remarks>
    /// Behind IIS the forwarded <c>X-Forwarded-Proto</c> already reports https, so redirection is
    /// a no-op for legitimate traffic and a genuine protection for anything arriving on the plain
    /// HTTP binding. On a PaaS host that terminates TLS and only tells the container a plain-HTTP
    /// <c>PORT</c>, there is no HTTPS port to redirect to and enabling it produces a loop.
    /// </remarks>
    public static bool ShouldRedirectToHttps(IConfiguration configuration)
    {
        var configured = configuration
            .GetSection(SecurityOptions.SectionName)
            .Get<SecurityOptions>()?.Https.RedirectToHttps;

        if (configured.HasValue)
        {
            return configured.Value;
        }

        return string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PORT"));
    }
}
