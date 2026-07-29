using FEA.URVP.Api.Middleware;

namespace FEA.URVP.Api.Configuration;

/// <summary>
/// HTTP request pipeline configuration.
/// </summary>
public static class MiddlewareConfiguration
{
    public static WebApplication ConfigureMiddlewarePipeline(this WebApplication app)
    {
        app.UseGlobalExceptionHandling();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseRequestLogging();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.MapControllers();

        return app;
    }
}
