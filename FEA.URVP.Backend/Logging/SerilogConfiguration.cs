using Serilog;
using Serilog.Events;

namespace FEA.URVP.Backend;

public static class SerilogConfiguration
{
    public static void AddSeriLog(this WebApplicationBuilder builder)
    {
        // dotnet run can look stuck if we log only to Seq, or if the Seq variables are missing.
        // Always log to the console, and add the Seq sink only when a server URL is configured.
        // On Windows these two names are machine variables (HKLM\...\Session Manager\Environment).
        // A user variable, a shell export, and launchSettings.json are ignored.
        // On Linux the process environment is the only source. ASPNETCORE_ENVIRONMENT selects
        // the appsettings file and is stamped on each event; it does not choose the Seq server.
        var seqServerUrl = ReadSeqEnvironmentVariable("SEQ_SERVER_URL");
        var apiKey = ReadSeqEnvironmentVariable("SEQ_API_KEY");

        builder.Logging.ClearProviders();

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console();

        // appsettings files set levels only. There is no WriteTo block, so they cannot
        // point the process at a Seq server. Microsoft and System stay at Warning unless
        // an environment file already named them.
        if (builder.Configuration["Serilog:MinimumLevel:Override:Microsoft"] is null)
        {
            loggerConfig.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
        }

        if (builder.Configuration["Serilog:MinimumLevel:Override:System"] is null)
        {
            loggerConfig.MinimumLevel.Override("System", LogEventLevel.Warning);
        }

        if (!string.IsNullOrWhiteSpace(seqServerUrl))
        {
            // Seq accepts a null key when ingestion is open. A blank key is the same as none.
            loggerConfig.WriteTo.Seq(
                serverUrl: seqServerUrl,
                apiKey: string.IsNullOrWhiteSpace(apiKey) ? null : apiKey);
        }

        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog(Log.Logger);

        if (string.IsNullOrWhiteSpace(seqServerUrl))
        {
            Log.Information("SEQ_SERVER_URL is not set. Logging to the console only.");
        }
    }

    private static string? ReadSeqEnvironmentVariable(string name)
    {
        return OperatingSystem.IsWindows()
            ? Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine)
            : Environment.GetEnvironmentVariable(name);
    }
}
