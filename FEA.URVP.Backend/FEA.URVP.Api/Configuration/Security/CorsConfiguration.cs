namespace FEA.URVP.Api.Configuration.Security;

/// <summary>
/// CORS policy.
/// </summary>
/// <remarks>
/// Production serves the frontend from this same process, so there is no legitimate cross-origin
/// caller and the expected configuration is an empty origin list, which denies every cross-origin
/// request. Origins only exist to support the local <c>next dev</c> split-port topology.
/// </remarks>
public static class CorsConfiguration
{
    public const string PolicyName = "UrvpCors";

    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var allowedOrigins = CorsOrigins.GetAllowedOrigins(configuration, environment.IsDevelopment());

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    // No origins configured: emit no Access-Control-Allow-Origin at all, so the
                    // browser rejects any cross-origin request. Never fall back to a wildcard,
                    // and never widen this for Development, which would make a hostile page on
                    // any origin able to drive a developer's authenticated session.
                    policy.WithOrigins();
                    return;
                }

                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    // Safe only because the origin list is explicit and wildcard-free.
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static void ValidateOrigins(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        var rejected = CorsOrigins.GetRejectedOrigins(configuration, environment.IsDevelopment());

        foreach (var origin in rejected)
        {
            logger.LogWarning(
                "Ignoring CORS origin {Origin}: production origins must be absolute HTTPS URLs "
                + "without wildcards.",
                origin);
        }

        var accepted = CorsOrigins.GetAllowedOrigins(configuration, environment.IsDevelopment());

        if (accepted.Length > 0 && !environment.IsDevelopment())
        {
            logger.LogWarning(
                "Cross-origin API access is enabled for {OriginCount} origin(s) in {Environment}. "
                + "The same-origin BFF deployment does not need this; remove Cors:AllowedOrigins "
                + "unless a separate frontend origin is genuinely required.",
                accepted.Length,
                environment.EnvironmentName);
        }
    }
}
