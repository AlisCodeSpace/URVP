namespace RICHConnect.Backend.Api.Configuration.Security
{
    /// <summary>
    /// Configuration for CORS policies
    /// </summary>
    public static class CorsConfiguration
    {
        /// <summary>
        /// Configure CORS policy
        /// </summary>
        public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddCors(options =>
            {
                // Add a named policy for the SPA
                options.AddPolicy("SpaCors", policy =>
                {
                    // For staging/production with embedded SPA, we don't need CORS since it's same-origin
                    // But keep configured origins for any external tools/clients or separate frontend deployments
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                        ?? (environment.IsDevelopment() ? new[] { "https://localhost:3000" } : Array.Empty<string>());
                    
                    // Validate that no wildcards are used in staging/production
                    if (environment.IsStaging() || environment.IsProduction())
                    {
                        var hasWildcards = allowedOrigins.Any(origin => origin.Contains("*"));
                        if (hasWildcards)
                        {
                            throw new InvalidOperationException("Wildcard origins are not allowed in staging or production environments. Use explicit origins only.");
                        }
                    }
                    
                    // Only configure WithOrigins if there are origins to allow
                    if (allowedOrigins.Length > 0)
                    {
                        policy
                            .WithOrigins(allowedOrigins) // Exact origins, no wildcards
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials(); // Critical for cookies
                    }
                    else
                    {
                        // No origins configured: deny cross-origin requests and block credentials
                        policy
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetIsOriginAllowed(_ => false) // Explicitly disallow all origins
                            .DisallowCredentials();
                    }
                });
                
                // Keep default policy for other endpoints
                options.AddDefaultPolicy(policy =>
                {
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
                        ?? (environment.IsDevelopment() ? new[] { "https://localhost:3000" } : Array.Empty<string>());
                    
                    // Validate that no wildcards are used in staging/production
                    if (environment.IsStaging() || environment.IsProduction())
                    {
                        var hasWildcards = allowedOrigins.Any(origin => origin.Contains("*"));
                        if (hasWildcards)
                        {
                            throw new InvalidOperationException("Wildcard origins are not allowed in staging or production environments. Use explicit origins only.");
                        }
                    }
                    
                    if (allowedOrigins.Length > 0)
                    {
                        policy
                            .WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials(); // Enable cookies for explicit origins
                    }
                    else
                    {
                        // Prevent credentialed requests when no origins are configured
                        policy
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetIsOriginAllowed(_ => false)
                            .DisallowCredentials();
                    }
                });
            });

            return services;
        }
    }
}
