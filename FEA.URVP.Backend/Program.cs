using FEA.URVP.Api.Configuration;
using FEA.URVP.Backend;
using Serilog;
using Serilog.Events;

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

builder.Logging.ClearProviders();

var seqServerUrl = builder.Configuration["Seq:ServerUrl"];
var seqApiKey = builder.Configuration["Seq:ApiKey"];

var loggerConfiguration = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId();

// Config-driven overrides win; these are only a safety net when appsettings
// does not define its own Microsoft/System levels.
if (builder.Configuration["Serilog:MinimumLevel:Override:Microsoft"] is null)
{
    loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
}

if (builder.Configuration["Serilog:MinimumLevel:Override:System"] is null)
{
    loggerConfiguration.MinimumLevel.Override("System", LogEventLevel.Warning);
}

loggerConfiguration.WriteTo.Console();

if (!string.IsNullOrWhiteSpace(seqServerUrl))
{
    loggerConfiguration.WriteTo.Seq(seqServerUrl, apiKey: seqApiKey);
}

Log.Logger = loggerConfiguration.CreateLogger();

builder.Host.UseSerilog(Log.Logger);

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
