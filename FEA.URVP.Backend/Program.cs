using FEA.URVP.Api.Configuration;
using FEA.URVP.Backend;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.ConfigureAllServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.ConfigureMiddlewarePipeline();

await app.StartAsync();

try
{
    await app.InitializeDatabaseAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database initialization failed. The API will keep running.");
}

await app.WaitForShutdownAsync();
