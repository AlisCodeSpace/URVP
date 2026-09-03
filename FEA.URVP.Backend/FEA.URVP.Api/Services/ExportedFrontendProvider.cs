using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using FEA.URVP.Api.Configuration.Security;
using Microsoft.Extensions.Options;

namespace FEA.URVP.Api.Services;

/// <summary>
/// Resolves and caches HTML documents from the statically exported Next.js app, and stamps a
/// per-response CSP nonce onto their script tags.
/// </summary>
/// <remarks>
/// Next.js prerenders inline bootstrap and flight-data scripts into every HTML file, so the
/// export cannot satisfy a <c>script-src 'self'</c> policy on its own. Rather than weaken CSP
/// with <c>'unsafe-inline'</c>, each HTML response gets a fresh nonce injected here.
/// Only HTML is handled; hashed assets under <c>/_next/static</c> are served by the static file
/// middleware and binary/API responses are never buffered through this type.
/// </remarks>
public sealed partial class ExportedFrontendProvider
{
    private readonly ConcurrentDictionary<string, CachedDocument> _cache = new(StringComparer.Ordinal);
    private readonly ILogger<ExportedFrontendProvider> _logger;
    private readonly string? _rootPath;

    public ExportedFrontendProvider(
        IOptions<SecurityOptions> options,
        IWebHostEnvironment environment,
        ILogger<ExportedFrontendProvider> logger)
    {
        _logger = logger;

        var frontend = options.Value.Frontend;
        if (!frontend.Enabled || string.IsNullOrWhiteSpace(frontend.RootPath))
        {
            return;
        }

        var resolved = Path.IsPathRooted(frontend.RootPath)
            ? frontend.RootPath
            : Path.Combine(environment.ContentRootPath, frontend.RootPath);

        resolved = Path.GetFullPath(resolved);

        if (!Directory.Exists(resolved))
        {
            _logger.LogWarning(
                "Exported frontend directory {RootPath} does not exist. The backend will serve API traffic only.",
                resolved);
            return;
        }

        if (!File.Exists(Path.Combine(resolved, "index.html")))
        {
            _logger.LogWarning(
                "Exported frontend directory {RootPath} has no index.html. The backend will serve API traffic only.",
                resolved);
            return;
        }

        _rootPath = resolved;
    }

    /// <summary>
    /// True when an exported frontend is present and should be served by this process.
    /// </summary>
    public bool IsEnabled => _rootPath is not null;

    public string? RootPath => _rootPath;

    /// <summary>
    /// Returns the HTML document for a browser navigation, with <paramref name="nonce"/> applied.
    /// Falls back to the exported <c>404.html</c> when the route has no prerendered document.
    /// </summary>
    public ExportedDocument? GetDocument(string requestPath, string nonce)
    {
        if (_rootPath is null)
        {
            return null;
        }

        foreach (var candidate in CandidatePaths(requestPath))
        {
            if (TryLoad(candidate, out var html))
            {
                return new ExportedDocument(ApplyNonce(html, nonce), StatusCodes.Status200OK);
            }
        }

        return TryLoad("404.html", out var notFound)
            ? new ExportedDocument(ApplyNonce(notFound, nonce), StatusCodes.Status404NotFound)
            : null;
    }

    /// <summary>
    /// Maps a request path onto the files Next.js emits for <c>output: 'export'</c>.
    /// </summary>
    private static IEnumerable<string> CandidatePaths(string requestPath)
    {
        var relative = requestPath.Trim('/');

        if (relative.Length == 0)
        {
            yield return "index.html";
            yield break;
        }

        if (relative.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            yield return relative;
            yield break;
        }

        yield return relative + ".html";
        yield return relative + "/index.html";
    }

    private bool TryLoad(string relativePath, out string html)
    {
        html = string.Empty;

        if (_rootPath is null || !TryResolveInsideRoot(relativePath, out var fullPath))
        {
            return false;
        }

        FileInfo info;
        try
        {
            info = new FileInfo(fullPath);
            if (!info.Exists)
            {
                return false;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Could not stat exported document {RelativePath}", relativePath);
            return false;
        }

        if (_cache.TryGetValue(relativePath, out var cached)
            && cached.LastWriteTimeUtc == info.LastWriteTimeUtc
            && cached.Length == info.Length)
        {
            html = cached.Html;
            return true;
        }

        try
        {
            var content = File.ReadAllText(fullPath);
            _cache[relativePath] = new CachedDocument(content, info.LastWriteTimeUtc, info.Length);
            html = content;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "Could not read exported document {RelativePath}", relativePath);
            return false;
        }
    }

    /// <summary>
    /// Confirms a candidate stays inside the export root. Request paths reach this type already
    /// URL-decoded, so traversal segments must be rejected on the resolved absolute path.
    /// </summary>
    private bool TryResolveInsideRoot(string relativePath, out string fullPath)
    {
        fullPath = string.Empty;

        if (_rootPath is null
            || relativePath.Length == 0
            || relativePath.Contains("..", StringComparison.Ordinal)
            || relativePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0
            || relativePath.Contains('\0'))
        {
            return false;
        }

        string combined;
        try
        {
            combined = Path.GetFullPath(Path.Combine(_rootPath, relativePath));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }

        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? _rootPath
            : _rootPath + Path.DirectorySeparatorChar;

        if (!combined.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        fullPath = combined;
        return true;
    }

    private static string ApplyNonce(string html, string nonce) =>
        ScriptTagRegex().Replace(html, $"<script nonce=\"{nonce}\"");

    [GeneratedRegex("<script(?=[\\s>])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ScriptTagRegex();

    private sealed record CachedDocument(string Html, DateTime LastWriteTimeUtc, long Length);
}

public sealed record ExportedDocument(string Html, int StatusCode);
