using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Entities.ValueLists;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Backend;

/// <summary>
/// Startup database initialization: migrations, catalog seed, and Development auth accounts.
/// </summary>
public static class DatabaseInitialization
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(DatabaseInitialization));
        var applyMigrations = app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", app.Environment.IsDevelopment());
        var seedCatalogs = app.Configuration.GetValue("Database:SeedCatalogsOnStartup", true);

        if (applyMigrations)
        {
            await ApplyMigrationsAsync(app, dbContext, logger);
        }
        else
        {
            logger.LogInformation(
                "Skipping automatic migrations for {Environment}. Set Database:ApplyMigrationsOnStartup to apply schema changes on startup.",
                app.Environment.EnvironmentName);
        }

        if (app.Environment.IsDevelopment())
        {
            try
            {
                await SeedDevAuthAccountsAsync(dbContext, logger);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Development auth account seeding skipped.");
            }
        }

        if (!seedCatalogs)
        {
            return;
        }

        try
        {
            await SeedValueListsAsync(dbContext, logger);
        }
        catch (Exception ex) when (app.Environment.IsDevelopment())
        {
            logger.LogWarning(ex, "Catalog seeding skipped.");
        }
    }

    private static async Task ApplyMigrationsAsync(
        WebApplication app,
        AppDbContext dbContext,
        ILogger logger)
    {
        if (app.Environment.IsDevelopment())
        {
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

            return;
        }

        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied.");
    }

    private static async Task SeedDevAuthAccountsAsync(AppDbContext dbContext, ILogger logger)
    {
        var seeded = 0;

        foreach (var account in DevAuthAccounts.All)
        {
            var existing = await dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == account.Email);

            if (existing is null)
            {
                var now = DateTime.UtcNow;
                dbContext.Users.Add(new User
                {
                    Email = account.Email,
                    Name = account.Name,
                    UserName = account.UserName,
                    Affiliation = DevAuthAccounts.Affiliation,
                    Role = account.Role,
                    RegisteredAt = now,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                seeded++;
                continue;
            }

            if (existing.Role == account.Role
                && existing.Name == account.Name
                && existing.UserName == account.UserName)
            {
                continue;
            }

            existing.Role = account.Role;
            existing.Name = account.Name;
            existing.UserName = account.UserName;
            existing.Affiliation = DevAuthAccounts.Affiliation;
            existing.UpdatedAt = DateTime.UtcNow;
            seeded++;
        }

        if (seeded == 0)
        {
            logger.LogInformation("Development auth accounts already up to date.");
            return;
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded / updated {Count} development auth account(s).", seeded);
    }

    private static async Task SeedValueListsAsync(AppDbContext dbContext, ILogger logger)
    {
        // Current frontend catalogs use the same research-area labels for areas and student interests.
        var source = ResearchAreaCatalog.Allowed.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

        var interests = await SeedKindAsync(dbContext, ValueListKind.ResearchInterest, source);
        var areas = await SeedKindAsync(dbContext, ValueListKind.ResearchArea, source);

        if (interests + areas == 0)
        {
            logger.LogInformation("Value lists already seeded.");
            return;
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation(
            "Seeded value lists: {InterestCount} research interest(s), {AreaCount} research area(s).",
            interests,
            areas);
    }

    private static async Task<int> SeedKindAsync(
        AppDbContext dbContext,
        ValueListKind kind,
        IReadOnlyList<string> names)
    {
        var existing = await dbContext.ValueListItems
            .Where(x => x.Kind == kind)
            .Select(x => x.Name)
            .ToListAsync();

        var existingSet = existing.ToHashSet(StringComparer.Ordinal);
        var now = DateTime.UtcNow;
        var sortOrder = existing.Count;
        var added = 0;

        foreach (var name in names)
        {
            if (existingSet.Contains(name))
            {
                continue;
            }

            dbContext.ValueListItems.Add(new ValueListItem
            {
                Kind = kind,
                Name = name,
                SortOrder = sortOrder++,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
            added++;
        }

        return added;
    }
}
