using FEA.URVP.Api.Configuration.Auth;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.News;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Entities.ValueLists;
using FEA.URVP.Domain.Entities.Workshops;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Backend;

/// <summary>
/// Startup database initialization: migrations, catalog seed, and demo auth accounts.
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

        // Gated by the same policy as the sign-in endpoint, so a Production database never
        // receives demo accounts that could be used to bypass SSO.
        if (DevSignInPolicy.IsEnabled(app.Configuration, app.Environment))
        {
            try
            {
                await SeedDevAuthAccountsAsync(dbContext, logger);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Demo auth account seeding skipped.");
            }
        }

        if (!seedCatalogs)
        {
            return;
        }

        try
        {
            await SeedValueListsAsync(dbContext, logger);
            await SeedNewsAndWorkshopsAsync(dbContext, logger);
            await SeedDefaultSemesterAsync(dbContext, logger);
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
            logger.LogInformation("Demo auth accounts already up to date.");
            return;
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded / updated {Count} demo auth account(s).", seeded);
    }

    private static async Task SeedValueListsAsync(AppDbContext dbContext, ILogger logger)
    {
        var interestSource = ResearchAreaCatalog.Allowed
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var activitySource = ResearchActivityTypeCatalog.Allowed
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var replacedActivities = await ReplaceStaleActivityTypesAsync(dbContext, logger);
        var interests = await SeedKindAsync(dbContext, ValueListKind.ResearchInterest, interestSource);
        var activities = await SeedKindAsync(
            dbContext,
            ValueListKind.ResearchActivityType,
            activitySource);

        if (replacedActivities + interests + activities == 0)
        {
            logger.LogInformation("Value lists already seeded.");
            return;
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation(
            "Seeded value lists: {InterestCount} research interest(s), {ActivityCount} research activity type(s).",
            interests,
            activities);
    }

    /// <summary>
    /// Kind 1 used to duplicate research areas. Replace those rows with activity types.
    /// </summary>
    private static async Task<int> ReplaceStaleActivityTypesAsync(AppDbContext dbContext, ILogger logger)
    {
        var existing = await dbContext.ValueListItems
            .Where(x => x.Kind == ValueListKind.ResearchActivityType)
            .ToListAsync();

        if (existing.Count == 0)
        {
            return 0;
        }

        var activityHits = existing.Count(x => ResearchActivityTypeCatalog.Allowed.Contains(x.Name));
        var areaHits = existing.Count(x => ResearchAreaCatalog.Allowed.Contains(x.Name));
        if (areaHits <= activityHits)
        {
            return 0;
        }

        dbContext.ValueListItems.RemoveRange(existing);
        await dbContext.SaveChangesAsync();
        logger.LogInformation(
            "Replaced {Count} duplicated research-area value(s) with research activity types.",
            existing.Count);
        return existing.Count;
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

    private static async Task SeedNewsAndWorkshopsAsync(AppDbContext dbContext, ILogger logger)
    {
        var newsCount = await dbContext.NewsArticles.CountAsync();
        var workshopCount = await dbContext.Workshops.CountAsync();
        if (newsCount > 0 && workshopCount > 0)
        {
            logger.LogInformation("News and workshops already seeded.");
            return;
        }

        var now = DateTime.UtcNow;
        var addedNews = 0;
        var addedWorkshops = 0;

        if (newsCount == 0)
        {
            foreach (var article in NewsSeedCatalog.Articles)
            {
                dbContext.NewsArticles.Add(new NewsArticle
                {
                    Slug = article.Slug,
                    Title = article.Title,
                    Excerpt = article.Excerpt,
                    Category = article.Category,
                    Author = article.Author,
                    Ticker = article.Ticker,
                    Body = [.. article.Body],
                    PublishedAt = article.PublishedAt,
                    Featured = article.Featured,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
                addedNews++;
            }
        }

        if (workshopCount == 0)
        {
            var sort = 0;
            foreach (var workshop in WorkshopSeedCatalog.Items)
            {
                dbContext.Workshops.Add(new Workshop
                {
                    Title = workshop.Title,
                    Date = workshop.Date,
                    Time = workshop.Time,
                    Location = workshop.Location,
                    Description = workshop.Description,
                    RegistrationUrl = workshop.RegistrationUrl,
                    SortOrder = sort++,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
                addedWorkshops++;
            }
        }

        if (addedNews + addedWorkshops == 0)
        {
            return;
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation(
            "Seeded content: {NewsCount} news article(s), {WorkshopCount} workshop(s).",
            addedNews,
            addedWorkshops);
    }

    /// <summary>
    /// Ensures a running cycle exists with the student application window open.
    /// </summary>
    private static async Task SeedDefaultSemesterAsync(AppDbContext dbContext, ILogger logger)
    {
        var now = DateTime.UtcNow;
        var semester = await dbContext.Semesters
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (semester is not null)
        {
            logger.LogInformation(
                "Semester {SemesterName} already exists (active={IsActive}, window open={WindowOpen}).",
                semester.Name,
                semester.IsActive,
                semester.IsApplicationWindowOpen(now));
            return;
        }

        dbContext.Semesters.Add(new Semester
        {
            Name = "Fall 2026–27",
            Description = "Default development cycle.",
            IsActive = true,
            CycleStart = now,
            ApplicationWindowStart = now,
            CreatedAt = now,
            UpdatedAt = now,
        });
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Seeded active semester Fall 2026–27 with applications open.");
    }
}
