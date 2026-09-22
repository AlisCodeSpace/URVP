using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace FEA.URVP.Backend;

public static class SerilogConfiguration
{
    public static void AddSeriLog(this WebApplicationBuilder builder)
    {
        var seqServerUrl = OperatingSystem.IsWindows()
            ? Environment.GetEnvironmentVariable("SEQ_SERVER_URL", EnvironmentVariableTarget.Machine)
            : Environment.GetEnvironmentVariable("SEQ_SERVER_URL");
        if (string.IsNullOrWhiteSpace(seqServerUrl))
            throw new InvalidOperationException("SEQ_SERVER_URL is not configured.");

        var apiKey = OperatingSystem.IsWindows()
            ? Environment.GetEnvironmentVariable("SEQ_API_KEY", EnvironmentVariableTarget.Machine)
            : Environment.GetEnvironmentVariable("SEQ_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SEQ_API_KEY is not configured.");

        var loggerConfig = GetCommonLoggerConfiguration(builder);
        if (builder.Environment.IsDevelopment())
        {
            loggerConfig.Enrich.WithRequestHeader("Authorization");
        }

        var logger = loggerConfig
            .WriteTo.Seq(seqServerUrl, apiKey: apiKey)
            .CreateLogger();

        Log.Logger = logger;
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog(logger);
    }

    private static LoggerConfiguration GetCommonLoggerConfiguration(WebApplicationBuilder builder)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console();

        if (builder.Configuration["Serilog:MinimumLevel:Override:Microsoft"] is null)
        {
            loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
        }

        if (builder.Configuration["Serilog:MinimumLevel:Override:System"] is null)
        {
            loggerConfiguration.MinimumLevel.Override("System", LogEventLevel.Warning);
        }

        return loggerConfiguration;
    }
}

internal static class RequestHeaderEnricherExtensions
{
    public static LoggerConfiguration WithRequestHeader(
        this LoggerEnrichmentConfiguration enrichment,
        string headerName) =>
        enrichment.With(new RequestHeaderEnricher(headerName));
}

internal sealed class RequestHeaderEnricher : ILogEventEnricher
{
    private readonly string _headerName;
    private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

    public RequestHeaderEnricher(string headerName) => _headerName = headerName;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var headers = _httpContextAccessor.HttpContext?.Request.Headers;
        if (headers is null || !headers.TryGetValue(_headerName, out var value))
        {
            return;
        }

        var text = value.ToString();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(_headerName, text));
    }
}
