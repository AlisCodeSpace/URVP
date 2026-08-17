using FEA.URVP.Api.Configuration;
using FEA.URVP.Backend;

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

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.ConfigureAllServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.ConfigureMiddlewarePipeline();

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = InitializeDatabaseInBackground(app);
});

await app.RunAsync();

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
