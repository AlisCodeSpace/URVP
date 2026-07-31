using FEA.URVP.Api.Configuration;
using FEA.URVP.Backend;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureAllServices(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.InitializeDatabaseAsync();
app.ConfigureMiddlewarePipeline();

app.Run();
