using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Tests.Security;

public sealed class ExportedFrontendProviderTests : IDisposable
{
    private readonly string _root;

    public ExportedFrontendProviderTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "urvp-export-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_root, "projects"));

        File.WriteAllText(
            Path.Combine(_root, "index.html"),
            "<html><head><script src=\"/_next/a.js\"></script></head><body><script>bootstrap()</script></body></html>");
        File.WriteAllText(Path.Combine(_root, "404.html"), "<html><body>not found</body></html>");
        File.WriteAllText(Path.Combine(_root, "projects.html"), "<html><body>list</body></html>");
        File.WriteAllText(Path.Combine(_root, "projects", "detail.html"), "<html><body>detail</body></html>");

        // A file that must never be reachable through a traversal attempt.
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "urvp-secret.html"), "TOP SECRET");
    }

    private ExportedFrontendProvider Provider(bool enabled = true, string? rootPath = null)
    {
        var options = new SecurityOptions();
        options.Frontend.Enabled = enabled;
        options.Frontend.RootPath = rootPath ?? _root;

        return new ExportedFrontendProvider(
            Options.Create(options),
            TestEnvironments.Production,
            NullLogger<ExportedFrontendProvider>.Instance);
    }

    [Fact]
    public void Root_path_maps_to_index_html()
    {
        var document = Provider().GetDocument("/", "nonce");

        Assert.NotNull(document);
        Assert.Equal(200, document.StatusCode);
        Assert.Contains("bootstrap()", document.Html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/projects", "list")]
    [InlineData("/projects/detail", "detail")]
    public void Query_param_routes_resolve_to_their_exported_html(string path, string marker)
    {
        var document = Provider().GetDocument(path, "nonce");

        Assert.NotNull(document);
        Assert.Equal(200, document.StatusCode);
        Assert.Contains(marker, document.Html, StringComparison.Ordinal);
    }

    [Fact]
    public void Unknown_route_returns_the_exported_404_with_a_404_status()
    {
        var document = Provider().GetDocument("/does-not-exist", "nonce");

        Assert.NotNull(document);
        Assert.Equal(404, document.StatusCode);
        Assert.Contains("not found", document.Html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/../urvp-secret")]
    [InlineData("/../../urvp-secret")]
    [InlineData("/projects/../../urvp-secret")]
    public void Traversal_attempts_cannot_escape_the_export_root(string path)
    {
        var document = Provider().GetDocument(path, "nonce");

        // Falls through to 404 rather than reading outside the root.
        Assert.Equal(404, document!.StatusCode);
        Assert.DoesNotContain("TOP SECRET", document.Html, StringComparison.Ordinal);
    }

    [Fact]
    public void Every_script_tag_receives_the_nonce()
    {
        var document = Provider().GetDocument("/", "abc123");

        Assert.NotNull(document);
        Assert.DoesNotContain("<script src", document.Html, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(document.Html, "nonce=\"abc123\""));
    }

    [Fact]
    public void Each_request_gets_its_own_nonce_even_though_the_file_is_cached()
    {
        var provider = Provider();

        var first = provider.GetDocument("/", "first-nonce");
        var second = provider.GetDocument("/", "second-nonce");

        Assert.Contains("first-nonce", first!.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("second-nonce", first.Html, StringComparison.Ordinal);
        Assert.Contains("second-nonce", second!.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("first-nonce", second.Html, StringComparison.Ordinal);
    }

    [Fact]
    public void Disabled_hosting_serves_nothing()
    {
        var provider = Provider(enabled: false);

        Assert.False(provider.IsEnabled);
        Assert.Null(provider.GetDocument("/", "nonce"));
    }

    [Fact]
    public void A_missing_export_directory_degrades_to_api_only()
    {
        var provider = Provider(rootPath: Path.Combine(_root, "no-such-directory"));

        Assert.False(provider.IsEnabled);
        Assert.Null(provider.GetDocument("/", "nonce"));
    }

    private static int CountOccurrences(string haystack, string needle)
    {
        var count = 0;
        var index = 0;

        while ((index = haystack.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += needle.Length;
        }

        return count;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
            File.Delete(Path.Combine(Path.GetTempPath(), "urvp-secret.html"));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Temp cleanup only.
        }
    }
}
