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
        var seqServerUrl = ReadEnvironmentVariable("SEQ_SERVER_URL");
        var apiKey = ReadEnvironmentVariable("SEQ_API_KEY");
        var writeToSeq = !string.IsNullOrWhiteSpace(seqServerUrl);

        if (writeToSeq && string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("SEQ_API_KEY is not configured.");

        var loggerConfig = GetCommonLoggerConfiguration(builder);
        if (builder.Environment.IsDevelopment())
        {
            loggerConfig.Enrich.WithRequestHeader("Authorization");
        }

        if (writeToSeq)
        {
            loggerConfig.WriteTo.Seq(seqServerUrl, apiKey: apiKey);
        }

        var logger = loggerConfig.CreateLogger();

        Log.Logger = logger;
        builder.Logging.ClearProviders();
        builder.Host.UseSerilog(logger);

        if (!writeToSeq)
        {
            Log.Warning("SEQ_SERVER_URL is not configured. Logging to the console only.");
        }
    }

    /// <summary>
    /// Process environment first, then the machine environment on Windows.
    /// Render and other Linux hosts only have a process environment.
    /// </summary>
    private static string? ReadEnvironmentVariable(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrWhiteSpace(value) || !OperatingSystem.IsWindows())
        {
            return value;
        }

        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
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
