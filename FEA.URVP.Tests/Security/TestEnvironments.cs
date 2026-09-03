using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace FEA.URVP.Tests.Security;

/// <summary>
/// Minimal <see cref="IWebHostEnvironment"/> and <see cref="IConfiguration"/> builders, so the
/// security policy classes can be exercised per environment without booting a host.
/// </summary>
internal static class TestEnvironments
{
    public static IWebHostEnvironment Production => Named(Environments.Production);

    public static IWebHostEnvironment Development => Named(Environments.Development);

    public static IWebHostEnvironment Staging => Named(Environments.Staging);

    public static IWebHostEnvironment Named(string environmentName) =>
        new StubWebHostEnvironment { EnvironmentName = environmentName };

    public static IConfiguration Config(params (string Key, string? Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(pair =>
                new KeyValuePair<string, string?>(pair.Key, pair.Value)))
            .Build();

    public static IConfiguration WithCorsOrigins(params string[] origins) =>
        Config(origins
            .Select((origin, index) => ($"Cors:AllowedOrigins:{index}", (string?)origin))
            .ToArray());

    private sealed class StubWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "FEA.URVP.Tests";
        public string WebRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider WebRootFileProvider { get; set; } =
            new PhysicalFileProvider(AppContext.BaseDirectory);
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } =
            new PhysicalFileProvider(AppContext.BaseDirectory);
    }
}
