using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FEA.URVP.Infrastructure.Data.Factories;

/// <summary>
/// Design-time factory for EF Core migrations tooling.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        // Prefer the host project content root when tooling runs from Infrastructure.
        var basePath = ResolveBasePath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = SqlConnectionString.Normalize(
            configuration.GetConnectionString("SqlServerConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'SqlServerConnection' was not found. " +
                "Set ConnectionStrings:SqlServerConnection in appsettings or via environment variables."));

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string ResolveBasePath()
    {
        var current = Directory.GetCurrentDirectory();

        // Infrastructure is nested under FEA.URVP.Backend; prefer the host project's appsettings.
        var candidates = new[]
        {
            current,
            Path.GetFullPath(Path.Combine(current, "..")),
            Path.GetFullPath(Path.Combine(current, "..", "FEA.URVP.Backend")),
            Path.GetFullPath(Path.Combine(current, "..", "..", "FEA.URVP.Backend"))
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            {
                return candidate;
            }
        }

        return current;
    }
}
