using Hangfire;
using Hangfire.Dashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using RICHConnect.Backend.Api.Middleware;
using RICHConnect.Backend.Api.Security;
using RICHConnect.Backend.Infrastructure.Data;
using System.Net.Http;
using AspNetCoreRateLimit;
using System.Security.Cryptography;
using System.Text;

namespace RICHConnect.Backend.Api.Configuration.Middleware
{
    /// <summary>
    /// Configuration for middleware pipeline setup
    /// </summary>
    public static class MiddlewareConfiguration
    {
        /// <summary>
        /// Configure the HTTP request pipeline with all middleware in the correct order
        /// </summary>
        public static async Task<IApplicationBuilder> ConfigureMiddlewarePipeline(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            // IMPORTANT: UseForwardedHeaders must be very early in the pipeline (before any middleware that reads client IP)
            // This ensures rate limiting, logging, and security policies see the real client IP, not the proxy IP
            app.UseForwardedHeaders();

            // Global exception handling should be one of the first middleware
            app.UseGlobalExceptionHandling();

            // Database initialization and seeding
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                
                try
                {
                    if (environment.IsDevelopment())
                    {
                        // Development: apply EF migrations to keep local schema in sync with the codebase.
                        // NOTE: EnsureCreated does NOT apply migrations and will leave existing dev DBs out of date.
                        logger.LogInformation("Applying database migrations for development environment...");
                        await dbContext.Database.MigrateAsync();
                        logger.LogInformation("Database migrations applied successfully for development.");
                    }
                    else if (environment.IsStaging())
                    {
                        // Staging: Apply migrations safely
                        logger.LogInformation("Applying database migrations for staging environment...");
                        await dbContext.Database.MigrateAsync();
                        logger.LogInformation("Database migrations applied successfully for staging.");
                    }
                    else if (environment.IsProduction())
                    {
                        // Production: Do NOT auto-apply migrations. Schema changes must be applied via
                        // a controlled process (e.g. dotnet ef database update in release pipeline or DBA).
                        logger.LogInformation("Production: database migrations are not applied at startup. Ensure schema is up to date before deployment.");
                    }
                    else
                    {
                        logger.LogInformation("Applying database migrations for environment: {Environment}...", environment.EnvironmentName);
                        await dbContext.Database.MigrateAsync();
                        logger.LogInformation("Database migrations applied successfully.");
                    }

                    // Ensure IsPublished exists on ResearchThemes (migration may be skipped if already in __EFMigrationsHistory)
                    await dbContext.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ResearchThemes') AND name = 'IsPublished')
                        ALTER TABLE [ResearchThemes] ADD [IsPublished] bit NOT NULL DEFAULT 0;
                    ");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to initialize database for environment: {Environment}", environment.EnvironmentName);
                    throw;
                }
                
                // Validate Azure AD metadata endpoint accessibility
                try
                {
                    var tenantId = configuration["AzureAd:TenantId"];
                    var instance = configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";
                    
                    if (!string.IsNullOrEmpty(tenantId))
                    {
                        var metadataUrl = $"{instance.TrimEnd('/')}/{tenantId}/v2.0/.well-known/openid-configuration";
                        logger.LogInformation("Validating Azure AD metadata endpoint accessibility: {MetadataUrl}", metadataUrl);
                        
                        using var httpClient = new HttpClient();
                        httpClient.Timeout = TimeSpan.FromSeconds(10);
                        
                        // In development, bypass SSL validation
                        if (environment.IsDevelopment())
                        {
                            var handler = new HttpClientHandler
                            {
                                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                            };
                            using var devHttpClient = new HttpClient(handler);
                            devHttpClient.Timeout = TimeSpan.FromSeconds(10);
                            
                            var response = await devHttpClient.GetAsync(metadataUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                logger.LogInformation("Azure AD metadata endpoint is accessible");
                            }
                            else
                            {
                                logger.LogWarning("Azure AD metadata endpoint returned status {StatusCode}. This may cause authentication issues.", response.StatusCode);
                            }
                        }
                        else
                        {
                            var response = await httpClient.GetAsync(metadataUrl);
                            if (response.IsSuccessStatusCode)
                            {
                                logger.LogInformation("Azure AD metadata endpoint is accessible");
                            }
                            else
                            {
                                logger.LogWarning("Azure AD metadata endpoint returned status {StatusCode}. This may cause authentication issues.", response.StatusCode);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to validate Azure AD metadata endpoint. Authentication may fail if the endpoint is not accessible. Error: {Message}", ex.Message);
                    logger.LogWarning("This is often caused by network connectivity issues, firewall restrictions, or the metadata endpoint being temporarily unavailable.");
                }
            }

            // Add security headers early in the pipeline (before CORS)
            app.UseSecurityHeaders(environment);

            // Add rate limiting after security headers but before CORS
            app.UseIpRateLimiting();

            // Apply CORS before other middleware - use named policy for SPA
            app.UseCors("SpaCors");

            // Enforce cookie security policies (Secure/HttpOnly) before auth components write cookies
            app.UseCookiePolicy();

            // Enable static files for serving uploaded files
            app.UseStaticFiles();

            // For Staging/Production: Serve the SPA from wwwroot/spa folder
            if (environment.IsStaging() || environment.IsProduction())
            {
                var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
                var spaEnabled = configuration.GetValue<bool?>("Spa:Enabled") ?? true;
                var configuredSpaPath = configuration["Spa:Path"] ?? Path.Combine("wwwroot", "spa");
                
                if (spaEnabled)
                {
                    // Support both absolute and relative paths (relative to ContentRootPath)
                    var spaRootPath = Path.IsPathRooted(configuredSpaPath)
                        ? configuredSpaPath
                        : Path.Combine(environment.ContentRootPath, configuredSpaPath);

                    if (Directory.Exists(spaRootPath))
                    {
                        var fileProvider = new PhysicalFileProvider(spaRootPath);
                    
                        // Enable default files (index.html) before static files
                        app.UseDefaultFiles(new DefaultFilesOptions
                        {
                            FileProvider = fileProvider,
                            RequestPath = "",
                            DefaultFileNames = new List<string> { "index.html" }
                        });
                    
                        // Serve SPA static files with proper cache headers
                        app.UseStaticFiles(new StaticFileOptions
                        {
                            FileProvider = fileProvider,
                            RequestPath = "",
                            OnPrepareResponse = ctx =>
                            {
                                // Cache static assets (JS, CSS, images, fonts) for 1 year
                                if (ctx.File.Name.EndsWith(".js") ||
                                    ctx.File.Name.EndsWith(".css") ||
                                    ctx.File.Name.EndsWith(".png") ||
                                    ctx.File.Name.EndsWith(".jpg") ||
                                    ctx.File.Name.EndsWith(".jpeg") ||
                                    ctx.File.Name.EndsWith(".svg") ||
                                    ctx.File.Name.EndsWith(".woff") ||
                                    ctx.File.Name.EndsWith(".woff2"))
                                {
                                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000,immutable");
                                }
                                // Don't cache route entrypoints / RSC payloads
                                else if (ctx.File.Name.EndsWith(".html") || ctx.File.Name.EndsWith(".txt"))
                                {
                                    ctx.Context.Response.Headers.Append("Cache-Control", "no-cache,no-store,must-revalidate");
                                }
                            }
                        });
                    }
                }
            }

            // Custom middleware
            app.UseRequestLogging();

            // Only use HTTPS redirection in production
            if (!environment.IsDevelopment())
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Routing and endpoints
            app.UseRouting();
            
            // Authentication - must come after routing
            app.UseAuthentication();
            
            // Authorization (must be between UseRouting and UseEndpoints)
            app.UseAuthorization();

            // Hangfire Dashboard for background job monitoring
            // Restricted to authenticated users with Admin role (or loopback requests as a safe fallback).
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new IDashboardAuthorizationFilter[]
                {
                    new HangfireDashboardAuthorizationFilter()
                }
            });
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
                // Map health check endpoints
                // /health - detailed health check (for monitoring/debugging)
                // /health/ready - readiness probe (all dependencies healthy)
                // /health/live - liveness probe (app is running, even if dependencies are degraded)
                endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = _ => true, // Include all health checks
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";
                        var result = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            status = report.Status.ToString(),
                            totalDuration = report.TotalDuration.TotalMilliseconds,
                            checks = report.Entries.Select(e => new
                            {
                                name = e.Key,
                                status = e.Value.Status.ToString(),
                                duration = e.Value.Duration.TotalMilliseconds,
                                description = e.Value.Description,
                                data = e.Value.Data,
                                exception = e.Value.Exception?.Message,
                                tags = e.Value.Tags
                            })
                        });
                        await context.Response.WriteAsync(result);
                    }
                });
                
                // Readiness probe - app is ready to accept traffic (all critical dependencies healthy)
                endpoints.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("critical"),
                    AllowCachingResponses = false
                });
                
                // Liveness probe - app is alive (returns healthy even if dependencies are degraded)
                endpoints.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = _ => false // No checks, just returns healthy if app is running
                });

                // SPA fallback endpoint with [AllowAnonymous] for staging/production.
                // CRITICAL: Without this, FallbackPolicy challenges ALL unmatched requests (including
                // /themes/* and other SPA routes), redirecting unauthenticated users to login.
                // This endpoint matches all non-API routes and explicitly allows anonymous access,
                // so themes and other public SPA pages work without authentication.
                if (environment.IsStaging() || environment.IsProduction())
                {
                    // Use a catch-all fallback pattern (not the default :nonfile fallback)
                    // so `/themes/{slug}/index.txt` RSC payload requests are matched too.
                    endpoints.MapFallback("{*path}", ServeSpaAsync(app.ApplicationServices, environment))
                        .AllowAnonymous();
                }
            });

            return app;
        }

        /// <summary>
        /// Serves the Next.js static SPA for unmatched routes. Handles both document requests
        /// and RSC payload requests (index.txt) for theme detail pages.
        /// </summary>
        private static Microsoft.AspNetCore.Http.RequestDelegate ServeSpaAsync(IServiceProvider services, IWebHostEnvironment environment)
        {
            return async context =>
            {
                var path = context.Request.Path.Value ?? string.Empty;

                // Exclude backend routes (should be matched by controllers, but guard anyway)
                if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("/hangfire", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                var configuration = services.GetRequiredService<IConfiguration>();
                var configuredSpaPath = configuration["Spa:Path"] ?? Path.Combine("wwwroot", "spa");
                var spaRootPath = Path.IsPathRooted(configuredSpaPath)
                    ? configuredSpaPath
                    : Path.Combine(environment.ContentRootPath, configuredSpaPath);

                // Next.js RSC prefetch requests /themes/{slug}/index.txt - only themes/_/index.txt exists.
                // Serve the placeholder RSC payload for any theme slug to fix 404 and CORS fallback.
                var isThemeRscPayload = path.StartsWith("/themes/", StringComparison.OrdinalIgnoreCase)
                    && path.EndsWith("/index.txt", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(path, "/themes/", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(path, "/themes", StringComparison.OrdinalIgnoreCase);

                // Theme detail pages (document or RSC): serve placeholder from themes/_/
                var isThemeDetail = path.StartsWith("/themes/", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(path, "/themes/", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(path, "/themes", StringComparison.OrdinalIgnoreCase);

                string indexPath;
                string contentType;
                if (isThemeRscPayload)
                {
                    indexPath = Path.Combine(spaRootPath, "themes", "_", "index.txt");
                    contentType = "text/x-component";
                }
                else if (isThemeDetail)
                {
                    indexPath = Path.Combine(spaRootPath, "themes", "_", "index.html");
                    contentType = "text/html";
                }
                else if (!Path.HasExtension(path))
                {
                    indexPath = Path.Combine(spaRootPath, "index.html");
                    contentType = "text/html";
                }
                else
                {
                    // Has extension but not theme RSC - let it 404 (static files should have served it)
                    context.Response.StatusCode = 404;
                    return;
                }

                if (File.Exists(indexPath))
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = contentType;
                    context.Response.Headers.Append("Cache-Control", "no-cache,no-store,must-revalidate");
                    await context.Response.SendFileAsync(indexPath);
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            };
        }

        /// <summary>
        /// Add request logging middleware
        /// </summary>
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }

        /// <summary>
        /// Add global exception handling middleware (should be one of the first middleware)
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }

        /// <summary>
        /// Add security headers middleware to all responses
        /// </summary>
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, IWebHostEnvironment environment)
        {
            return app.Use(async (context, next) =>
            {
                // Only buffer + rewrite HTML documents to inject CSP nonces.
                // We do NOT want to buffer API responses or large downloads.
                var acceptsHtml = context.Request.Headers.Accept.ToString()
                    .Contains("text/html", StringComparison.OrdinalIgnoreCase);
                var isDocumentRequest = acceptsHtml
                    && !context.Request.Path.StartsWithSegments("/api")
                    && !context.Request.Path.StartsWithSegments("/swagger")
                    && !context.Request.Path.StartsWithSegments("/health");

                // Generate a per-response nonce for inline scripts (HTML only).
                // Base64 nonces are valid per CSP spec.
                var nonceBytes = RandomNumberGenerator.GetBytes(16);
                var cspNonce = Convert.ToBase64String(nonceBytes);
                context.Items["csp_nonce"] = cspNonce;

                Stream? originalBody = null;
                MemoryStream? buffer = null;
                if (!environment.IsDevelopment() && isDocumentRequest)
                {
                    originalBody = context.Response.Body;
                    buffer = new MemoryStream();
                    context.Response.Body = buffer;
                }

                // X-Content-Type-Options: Prevent MIME type sniffing
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

                // Referrer-Policy: Control referrer information
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

                // X-Frame-Options: Prevent clickjacking (SAMEORIGIN allows same-origin framing for SPA)
                context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");

                // Permissions-Policy: Restrict browser features
                context.Response.Headers.Append("Permissions-Policy", 
                    "camera=(), microphone=(), geolocation=(), payment=(), usb=(), magnetometer=(), gyroscope=(), accelerometer=()");

                // Content-Security-Policy (CSP): Mitigate XSS attacks.
                //
                // IMPORTANT:
                // - In staging/production this API also serves the statically-exported Next.js SPA from `wwwroot/spa`.
                // - Next.js static export includes a few inline scripts needed for hydration/runtime.
                // - A strict `script-src 'self'` CSP will block those inline scripts and break the SPA.
                //
                // To keep CSP effective while allowing the SPA to run, we scope the relaxed policy to HTML
                // responses only (documents) and keep `script-src-attr 'none'` to still block inline event
                // handlers like `onclick=...`.
                context.Response.OnStarting(() =>
                {
                    // Remove technology-identifying headers to reduce server fingerprinting.
                    context.Response.Headers.Remove("Server");
                    context.Response.Headers.Remove("X-Powered-By");
                    context.Response.Headers.Remove("X-AspNet-Version");
                    context.Response.Headers.Remove("X-AspNetMvc-Version");

                    var contentType = context.Response.ContentType ?? string.Empty;
                    var isHtmlDocument = contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);
                    var hasCacheControl = context.Response.Headers.ContainsKey("Cache-Control");

                    // Set a conservative default for dynamic responses. Static files and file download
                    // endpoints set explicit cache headers earlier and are left unchanged.
                    if (!hasCacheControl)
                    {
                        context.Response.Headers["Cache-Control"] = "no-store";
                    }

                    // CSP reporting endpoint declaration (required by some scanners/policies).
                    context.Response.Headers["Report-To"] =
                        "{\"group\":\"csp-endpoint\",\"max_age\":10886400,\"endpoints\":[{\"url\":\"/api/security/csp-report\"}]}";
                    context.Response.Headers["Reporting-Endpoints"] = "csp-endpoint=\"/api/security/csp-report\"";

                    var nonce = context.Items.TryGetValue("csp_nonce", out var nonceObj) ? nonceObj as string : null;

                    var cspHeader = environment.IsDevelopment()
                        ? "default-src 'self'; base-uri 'self'; object-src 'none'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; script-src-attr 'none'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob: https:; font-src 'self' data:; connect-src 'self' https://login.microsoftonline.com https://aubb2c.b2clogin.com; frame-src https://login.microsoftonline.com https://aubb2c.b2clogin.com;"
                        : isHtmlDocument
                            ? $"default-src 'self'; base-uri 'self'; object-src 'none'; script-src 'self' 'nonce-{nonce}'; script-src-attr 'none'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob: https:; font-src 'self' data:; connect-src 'self' https://login.microsoftonline.com https://aubb2c.b2clogin.com; frame-src https://login.microsoftonline.com https://aubb2c.b2clogin.com; frame-ancestors 'self'; report-to csp-endpoint;"
                            : "default-src 'self'; base-uri 'self'; object-src 'none'; script-src 'self'; script-src-attr 'none'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob: https:; font-src 'self' data:; connect-src 'self' https://login.microsoftonline.com https://aubb2c.b2clogin.com; frame-src https://login.microsoftonline.com https://aubb2c.b2clogin.com; frame-ancestors 'self'; report-to csp-endpoint;";

                    context.Response.Headers["Content-Security-Policy"] = cspHeader;
                    return Task.CompletedTask;
                });

                // X-XSS-Protection: Legacy header (modern browsers ignore, but some tools expect it)
                // Note: This is deprecated but harmless to include
                if (!environment.IsDevelopment())
                {
                    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                }

                await next();

                // If we buffered an HTML response, inject the CSP nonce into inline <script> tags.
                if (buffer != null && originalBody != null)
                {
                    try
                    {
                        buffer.Position = 0;
                        var contentType = context.Response.ContentType ?? string.Empty;
                        var isHtml = contentType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase);

                        if (isHtml)
                        {
                            using var reader = new StreamReader(buffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
                            var html = await reader.ReadToEndAsync();

                            var nonce = context.Items["csp_nonce"] as string;
                            if (!string.IsNullOrEmpty(nonce))
                            {
                                // Add nonce to any <script> tag that doesn't already have one.
                                // This enables inline Next.js bootstrapping scripts without using 'unsafe-inline'.
                                html = System.Text.RegularExpressions.Regex.Replace(
                                    html,
                                    @"<script(?![^>]*\snonce=)([^>]*)>",
                                    m => $"<script nonce=\"{nonce}\"{m.Groups[1].Value}>",
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                            }

                            var outBytes = Encoding.UTF8.GetBytes(html);
                            context.Response.Headers.ContentLength = outBytes.Length;
                            context.Response.Body = originalBody;
                            await context.Response.Body.WriteAsync(outBytes, 0, outBytes.Length);
                        }
                        else
                        {
                            // Not HTML after all; just pass through.
                            buffer.Position = 0;
                            context.Response.Body = originalBody;
                            await buffer.CopyToAsync(context.Response.Body);
                        }
                    }
                    finally
                    {
                        context.Response.Body = originalBody;
                        await buffer.DisposeAsync();
                    }
                }
            });
        }
    }
}
