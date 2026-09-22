using FEA.URVP.Api.Configuration;
using FEA.URVP.Api.Configuration.Auth;
using FEA.URVP.Backend;
using Serilog;

// Render (and similar Linux PaaS) often exhaust inotify watches. Config file
// reload is not needed outside local Development.
if (!string.Equals(
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
        "Development",
        StringComparison.OrdinalIgnoreCase))
{
    Environment.SetEnvironmentVariable("DOTNET_HOSTBUILDER_RELOADCONFIGONCHANGE", "false");
}

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.local.json",
    optional: true,
    reloadOnChange: false);

builder.AddSeriLog();

builder.Services.AddUrvpAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddAuthorizationPolicies();

try
{
    Log.Information(
        "Starting {Application} in {EnvironmentName} environment",
        builder.Environment.ApplicationName,
        builder.Environment.EnvironmentName);

    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrWhiteSpace(port))
    {
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    }

    var maxUploadBytes = builder.Configuration.GetValue(
        "FileStorage:MaxTotalSizeBytes",
        FEA.URVP.Domain.Catalog.FileStorageCatalog.MaxTotalSizeBytes);
    if (maxUploadBytes <= 0)
    {
        maxUploadBytes = FEA.URVP.Domain.Catalog.FileStorageCatalog.MaxTotalSizeBytes;
    }

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = maxUploadBytes;

        // Kestrel advertises "Server: Kestrel" by default. Removing it here covers the direct
        // binding; IIS and any other reverse proxy must strip their own equivalents.
        options.AddServerHeader = false;
    });

    builder.Services.ConfigureAllServices(builder.Configuration, builder.Environment);

    var app = builder.Build();

    app.ConfigureMiddlewarePipeline();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        _ = InitializeDatabaseInBackground(app);
    });

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}

static async Task InitializeDatabaseInBackground(WebApplication app)
{
    try
    {
        await app.InitializeDatabaseAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database initialization failed. The API will keep running.");
    }
}
