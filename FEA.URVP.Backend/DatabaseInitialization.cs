using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Entities.ValueLists;
using FEA.URVP.Domain.Enums;
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

            await SeedDevAuthAccountsAsync(dbContext, logger);
            await SeedValueListsAsync(dbContext, logger);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Database initialization skipped. Create an initial migration when the schema is ready.");
        }
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
