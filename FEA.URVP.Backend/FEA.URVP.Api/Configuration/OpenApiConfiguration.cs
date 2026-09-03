using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Api.Configuration;

/// <summary>
/// API schema exposure.
/// </summary>
/// <remarks>
/// The schema enumerates every route, parameter and payload shape, which is reconnaissance for an
/// attacker. It is never registered in Production, is anonymous only in Development, and requires
/// an authenticated administrator anywhere in between (for example Staging).
/// </remarks>
public static class OpenApiConfiguration
{
    public static IServiceCollection AddApiSchema(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        if (environment.IsProduction())
        {
            return services;
        }

        return services.AddOpenApi();
    }

    public static WebApplication MapApiSchema(this WebApplication app)
    {
        if (app.Environment.IsProduction())
        {
            return app;
        }

        var endpoint = app.MapOpenApi();

        if (app.Environment.IsDevelopment())
        {
            endpoint.AllowAnonymous();
            return app;
        }

        endpoint.RequireAuthorization(policy => policy
            .RequireAuthenticatedUser()
            .RequireRole(nameof(UserRole.Admin)));

        return app;
    }
}
