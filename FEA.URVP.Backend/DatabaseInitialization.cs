using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Backend;

/// <summary>
/// Optional startup database initialization (Development only).
/// </summary>
public static class DatabaseInitialization
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseInitialization));

        if (!app.Environment.IsDevelopment())
        {
            logger.LogInformation(
                "Skipping automatic migrations for {Environment}. Apply schema changes through a controlled deployment process.",
                app.Environment.EnvironmentName);
            return;
        }

        try
        {
            logger.LogInformation("Applying database migrations for Development...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Database initialization skipped. Create an initial migration when the schema is ready.");
        }
    }
}
