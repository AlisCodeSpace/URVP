using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;
using MediatR;
using RICHConnect.Backend.Application.Commands.Auth.AzureAd;
using RICHConnect.Backend.Application.Commands.Auth.AzureB2C;
using RICHConnect.Backend.Application.Services.FMIS;
using RICHConnect.Backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace RICHConnect.Backend.Api.Configuration.Auth
{
    /// <summary>
    /// OIDC event handlers for user provisioning and claims enrichment
    /// </summary>
    public static class OidcEventHandlers
    {
        private static async Task EnsureAuthorizationEndpointAsync(RedirectContext context, ILogger logger, string providerName)
        {
            // Ensure configuration is loaded before redirecting. If the discovered configuration omits AuthorizationEndpoint
            // (observed in some environments), fall back to a known authorize endpoint format based on Authority.
            try
            {
                var configurationManager = context.Options.ConfigurationManager;
                if (configurationManager == null)
                {
                    logger.LogWarning("{Provider}: ConfigurationManager is null. Metadata discovery may fail.", providerName);
                    return;
                }

                var config = await configurationManager.GetConfigurationAsync(context.HttpContext.RequestAborted);
                if (config == null)
                {
                    logger.LogError("{Provider}: OpenIdConnect configuration is null. Cannot proceed with redirect.", providerName);
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    return;
                }

                if (string.IsNullOrEmpty(config.AuthorizationEndpoint))
                {
                    logger.LogWarning(
                        "{Provider}: Authorization endpoint is missing from OpenIdConnect configuration. Falling back to known authorize endpoint format. Issuer: {Issuer}, TokenEndpoint: {TokenEndpoint}, UserInfoEndpoint: {UserInfoEndpoint}",
                        providerName, config.Issuer, config.TokenEndpoint, config.UserInfoEndpoint);

                    var authority = context.Options.Authority?.TrimEnd('/');
                    if (string.IsNullOrWhiteSpace(authority))
                    {
                        logger.LogError("{Provider}: Cannot compute fallback authorization endpoint because Authority is empty.", providerName);
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        return;
                    }

                    // Authority is configured as: https://.../v2.0
                    // Authorization endpoint is:  https://.../oauth2/v2.0/authorize
                    var authorityBase = authority.EndsWith("/v2.0", StringComparison.OrdinalIgnoreCase)
                        ? authority[..^5]
                        : authority;

                    var fallbackAuthorizeEndpoint = $"{authorityBase}/oauth2/v2.0/authorize";
                    context.ProtocolMessage.IssuerAddress = fallbackAuthorizeEndpoint;
                    logger.LogWarning("{Provider}: Using fallback authorization endpoint: {AuthorizationEndpoint}", providerName, fallbackAuthorizeEndpoint);
                    return;
                }

                logger.LogInformation(
                    "{Provider}: OpenIdConnect configuration loaded successfully. AuthorizationEndpoint: {AuthorizationEndpoint}",
                    providerName, config.AuthorizationEndpoint);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "{Provider}: Error loading OpenIdConnect configuration during redirect. Exception: {ExceptionType}, Message: {Message}",
                    providerName, ex.GetType().Name, ex.Message);
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        }

        /// <summary>
        /// Creates OIDC events for Azure AD authentication
        /// </summary>
        public static OpenIdConnectEvents CreateAzureAdOidcEvents(IConfiguration configuration)
        {
            var events = new OpenIdConnectEvents();
            
            events.OnRedirectToIdentityProvider = async context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.OidcEventHandlers");
                
                logger.LogInformation("Redirecting to Azure AD identity provider. Authority: {Authority}, ClientId: {ClientId}, MetadataAddress: {MetadataAddress}", 
                    context.Options.Authority, context.Options.ClientId, context.Options.MetadataAddress);

                await EnsureAuthorizationEndpointAsync(context, logger, "Azure AD");
            };
            
            events.OnTokenValidated = async context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.OidcEventHandlers");
                
                try
                {
                    // Extract claims from Azure AD token
                    var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value
                        ?? context.Principal?.FindFirst("preferred_username")?.Value
                        ?? context.Principal?.FindFirst("email")?.Value;

                    var name = context.Principal?.FindFirst("name")?.Value
                        ?? context.Principal?.FindFirst(ClaimTypes.Name)?.Value
                        ?? email;

                    if (string.IsNullOrEmpty(email))
                    {
                        logger.LogWarning("Azure AD token validation failed: no email claim found");
                        context.Fail("Email claim not found in Azure AD token");
                        return;
                    }

                    logger.LogInformation("Processing Azure AD OIDC sign-in for user: {Email}", email);

                    // Check FMIS enforcement
                    var fmisEnforcement = configuration.GetValue<bool>("Fmis:EnforceForAzureAdLogins", false);
                    UserRole? roleOverride = null;
                    
                    if (fmisEnforcement)
                    {
                        var fmisChecker = context.HttpContext.RequestServices.GetRequiredService<IFmisMembershipChecker>();
                        var membershipResult = await fmisChecker.CheckMembershipAsync(email, context.HttpContext.RequestAborted);

                        if (!membershipResult.IsAllowed)
                        {
                            logger.LogWarning("User {Email} denied: not in FMIS and not in special allowed list", email);
                            context.Fail("Access denied: You must be a faculty member to access this system");
                            
                            // Clear any existing cookie session
                            await context.HttpContext.SignOutAsync(AuthenticationConfiguration.CookieScheme);
                            
                            // Redirect to sign-in page with error
                            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                            var frontendOrigin = allowedOrigins.FirstOrDefault() ?? "/";
                            var redirectUrl = frontendOrigin.StartsWith("http") 
                                ? $"{frontendOrigin}/sign-in?error=access_denied&reason=not_allowed"
                                : "/sign-in?error=access_denied&reason=not_allowed";
                            context.Response.Redirect(redirectUrl);
                            context.HandleResponse();
                            return;
                        }
                        
                        // Store role override if provided
                        roleOverride = membershipResult.EffectiveRole;
                        logger.LogInformation("User {Email} allowed via {Source} with role {Role}", 
                            email, membershipResult.Source, roleOverride);
                    }
                    else
                    {
                        logger.LogInformation(
                            "FMIS enforcement is disabled (Fmis:EnforceForAzureAdLogins=false). Checking SpecialAllowedUsers for role override for user {Email}.",
                            email);
                        
                        // Even when FMIS enforcement is disabled, check SpecialAllowedUsers for role assignment
                        var specialAllowedUsers = configuration.GetSection("Fmis:SpecialAllowedUsers").Get<SpecialAllowedUserConfig[]>() ?? Array.Empty<SpecialAllowedUserConfig>();
                        var specialUser = specialAllowedUsers.FirstOrDefault(u => 
                            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                        
                        if (specialUser != null)
                        {
                            // Parse role from SpecialAllowedUsers
                            roleOverride = ParseRoleFromString(specialUser.Role);
                            logger.LogInformation("User {Email} found in SpecialAllowedUsers with role {Role}", 
                                email, roleOverride);
                        }
                    }

                    // Provision or update user with role override
                    var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
                    var safeName = string.IsNullOrWhiteSpace(name) ? email : name;
                    var user = await mediator.Send(new UpsertAzureAdUserCommand(email, safeName, null, roleOverride));

                    logger.LogInformation("User provisioned: {UserId}, Role: {Role}", user.Id, user.Role);

                    // Build claims identity with application-specific claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Role, user.Role.ToString()),
                        new Claim("userId", user.Id.ToString()),
                        new Claim("role", user.Role.ToString()),
                        new Claim("numericRole", ((int)user.Role).ToString())
                    };

                    // Add profile image if available
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    {
                        claims.Add(new Claim("profileImageUrl", user.ProfileImageUrl));
                    }

                    // Create new claims identity
                    var claimsIdentity = new ClaimsIdentity(claims, context.Principal?.Identity?.AuthenticationType);
                    context.Principal = new ClaimsPrincipal(claimsIdentity);

                    logger.LogInformation("Claims enriched for user {Email}: userId={UserId}, role={Role}", 
                        email, user.Id, user.Role);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during Azure AD OIDC token validation and user provisioning");
                    context.Fail("An error occurred during authentication");
                }
            };

            events.OnAuthenticationFailed = async context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.OidcEventHandlers");
                logger.LogError(context.Exception, "Azure AD OIDC authentication failed. Exception: {ExceptionType}, Message: {Message}", 
                    context.Exception?.GetType().Name, context.Exception?.Message);
                
                // Log configuration details for debugging
                logger.LogError("OIDC Configuration - Authority: {Authority}, ClientId: {ClientId}, CallbackPath: {CallbackPath}, MetadataAddress: {MetadataAddress}",
                    context.Options.Authority, context.Options.ClientId, context.Options.CallbackPath, context.Options.MetadataAddress);
                
                // Check if this is a configuration/metadata error
                if (context.Exception is InvalidOperationException invalidOpEx && 
                    (invalidOpEx.Message.Contains("authorization endpoint") || invalidOpEx.Message.Contains("configuration")))
                {
                    logger.LogError("OpenIdConnect metadata discovery failed. This usually means:");
                    logger.LogError("1. The metadata endpoint is not accessible: {MetadataAddress}", context.Options.MetadataAddress);
                    logger.LogError("2. Network connectivity issues or firewall blocking the request");
                    logger.LogError("3. The ConfigurationManager failed to load the metadata document");
                    
                    // Try to get the configuration to see if it's loaded
                    try
                    {
                        var configManager = context.Options.ConfigurationManager;
                        if (configManager != null)
                        {
                            var config = await configManager.GetConfigurationAsync(context.HttpContext.RequestAborted);
                            if (config == null)
                            {
                                logger.LogError("ConfigurationManager returned null configuration. Metadata document was not loaded.");
                            }
                            else
                            {
                                logger.LogError("Configuration is available but authorization endpoint is missing: {AuthorizationEndpoint}", 
                                    config.AuthorizationEndpoint ?? "NULL");
                            }
                        }
                        else
                        {
                            logger.LogError("ConfigurationManager is null. This should not happen.");
                        }
                    }
                    catch (Exception configEx)
                    {
                        logger.LogError(configEx, "Error attempting to retrieve configuration: {Message}", configEx.Message);
                    }
                }
                
                // Redirect to frontend error page
                var frontendCallbackPath = configuration["AzureAd:FrontendCallbackPath"] ?? "/auth/callback";
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                var frontendOrigin = allowedOrigins.FirstOrDefault() ?? "";
                var redirectUrl = !string.IsNullOrEmpty(frontendOrigin) 
                    ? $"{frontendOrigin}{frontendCallbackPath}?error=authentication_failed"
                    : $"{frontendCallbackPath}?error=authentication_failed";
                context.Response.Redirect(redirectUrl);
                context.HandleResponse();
            };

            events.OnRemoteFailure = context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.OidcEventHandlers");
                
                // Enhanced logging for state protection failures
                var isStateProtectionError = context.Failure?.Message?.Contains("unprotect", StringComparison.OrdinalIgnoreCase) == true
                    || context.Failure?.Message?.Contains("State", StringComparison.OrdinalIgnoreCase) == true;
                
                if (isStateProtectionError)
                {
                    logger.LogError(context.Failure, 
                        "Azure AD OIDC state protection failure. This usually indicates: " +
                        "1) Application was restarted between challenge and callback, " +
                        "2) Data protection keys changed or weren't loaded, " +
                        "3) State parameter expired or was corrupted. " +
                        "Exception: {ExceptionType}, Message: {Message}",
                        context.Failure?.GetType().Name, context.Failure?.Message);
                    
                    // Log request details for debugging (SECURITY: redact query string to avoid leaking tokens/state)
                    var queryKeys = string.Join(", ", context.Request.Query.Keys);
                    logger.LogError("Request details - Path: {Path}, QueryKeys: [{QueryKeys}], Method: {Method}, HasFormData: {HasFormData}",
                        context.Request.Path,
                        queryKeys,
                        context.Request.Method,
                        context.Request.HasFormContentType);
                    
                    // Check if state parameter exists in form or query
                    if (context.Request.HasFormContentType)
                    {
                        var stateFromForm = context.Request.Form["state"].FirstOrDefault();
                        logger.LogError("State parameter from form: {State} (Length: {Length})",
                            string.IsNullOrEmpty(stateFromForm) ? "MISSING" : "PRESENT",
                            stateFromForm?.Length ?? 0);
                    }
                    
                    var stateFromQuery = context.Request.Query["state"].FirstOrDefault();
                    logger.LogError("State parameter from query: {State} (Length: {Length})",
                        string.IsNullOrEmpty(stateFromQuery) ? "MISSING" : "PRESENT",
                        stateFromQuery?.Length ?? 0);
                }
                else
                {
                    logger.LogError(context.Failure, "Azure AD OIDC remote failure");
                }
                
                // Redirect to frontend error page with more specific error code
                var frontendCallbackPath = configuration["AzureAd:FrontendCallbackPath"] ?? "/auth/callback";
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                var frontendOrigin = allowedOrigins.FirstOrDefault() ?? "";
                
                var errorCode = isStateProtectionError ? "state_protection_failed" : "remote_failure";
                var redirectUrl = !string.IsNullOrEmpty(frontendOrigin) 
                    ? $"{frontendOrigin}{frontendCallbackPath}?error={errorCode}"
                    : $"{frontendCallbackPath}?error={errorCode}";
                context.Response.Redirect(redirectUrl);
                context.HandleResponse();
                
                return Task.CompletedTask;
            };
            
            return events;
        }

        /// <summary>
        /// Creates OIDC events for Azure B2C authentication
        /// </summary>
        public static OpenIdConnectEvents CreateAzureB2COidcEvents(IConfiguration configuration)
        {
            var events = new OpenIdConnectEvents();

            events.OnRedirectToIdentityProvider = async context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.OidcEventHandlers");

                logger.LogInformation("Redirecting to Azure B2C identity provider. Authority: {Authority}, ClientId: {ClientId}, MetadataAddress: {MetadataAddress}",
                    context.Options.Authority, context.Options.ClientId, context.Options.MetadataAddress);

                await EnsureAuthorizationEndpointAsync(context, logger, "Azure B2C");
            };
            
            events.OnTokenValidated = async context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.OidcEventHandlers");
                
                try
                {
                    // Extract email from B2C token - handle common B2C variations
                    string? email = null;
                    
                    // Try standard email claim
                    email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value
                        ?? context.Principal?.FindFirst("email")?.Value
                        ?? context.Principal?.FindFirst("preferred_username")?.Value;
                    
                    // B2C may return emails as an array in "emails" claim
                    if (string.IsNullOrEmpty(email))
                    {
                        var emailsClaim = context.Principal?.FindFirst("emails");
                        if (emailsClaim != null)
                        {
                            try
                            {
                                // Try to parse as JSON array
                                var emailsArray = JsonSerializer.Deserialize<string[]>(emailsClaim.Value);
                                email = emailsArray?.FirstOrDefault();
                            }
                            catch
                            {
                                // If not JSON, treat as single value
                                email = emailsClaim.Value;
                            }
                        }
                    }

                    // Extract name from B2C token
                    var name = context.Principal?.FindFirst("name")?.Value
                        ?? context.Principal?.FindFirst(ClaimTypes.Name)?.Value
                        ?? context.Principal?.FindFirst("given_name")?.Value
                        ?? email;

                    if (string.IsNullOrEmpty(email))
                    {
                        logger.LogWarning("Azure B2C token validation failed: no email claim found");
                        context.Fail("Email claim not found in Azure B2C token");
                        return;
                    }

                    logger.LogInformation("Processing Azure B2C OIDC sign-in for user: {Email}", email);

                    // Provision or update user (B2C users are always CommunityPartner)
                    var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
                    var user = await mediator.Send(new UpsertAzureB2CUserCommand(email, name ?? email, null));

                    logger.LogInformation("User provisioned: {UserId}, Role: {Role}", user.Id, user.Role);

                    // Build claims identity with application-specific claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim("nameid", user.Id.ToString()), // For compatibility
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Role, user.Role.ToString()),
                        new Claim("userId", user.Id.ToString()),
                        new Claim("role", user.Role.ToString()),
                        new Claim("rc_role", ((int)user.Role).ToString()), // Numeric role for frontend
                        new Claim("numericRole", ((int)user.Role).ToString())
                    };

                    // Add profile image if available
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    {
                        claims.Add(new Claim("profileImageUrl", user.ProfileImageUrl));
                    }

                    // Create new claims identity
                    var claimsIdentity = new ClaimsIdentity(claims, context.Principal?.Identity?.AuthenticationType);
                    context.Principal = new ClaimsPrincipal(claimsIdentity);

                    logger.LogInformation("Claims enriched for user {Email}: userId={UserId}, role={Role}", 
                        email, user.Id, user.Role);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during Azure B2C OIDC token validation and user provisioning");
                    context.Fail("An error occurred during authentication");
                }
            };

            events.OnTicketReceived = async context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.OidcEventHandlers");
                
                logger.LogInformation("Azure B2C ticket received - signing into cookie scheme");
                
                // Get the return URL from authentication properties
                var returnUrl = context.Properties?.RedirectUri;
                if (string.IsNullOrEmpty(returnUrl))
                {
                    // Fallback to frontend callback path
                    var frontendCallbackPath = configuration["AzureB2C:FrontendCallbackPath"] ?? "/auth/callback";
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                    var frontendOrigin = allowedOrigins.FirstOrDefault() ?? "";
                    returnUrl = !string.IsNullOrEmpty(frontendOrigin) 
                        ? $"{frontendOrigin}{frontendCallbackPath}"
                        : frontendCallbackPath;
                }
                
                // Check if user has rejected profile - block login if so
                var userIdClaim = context.Principal?.FindFirst("userId")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
                {
                    var dbContext = context.HttpContext.RequestServices.GetRequiredService<RICHConnect.Backend.Infrastructure.Data.AppDbContext>();
                    var partner = await dbContext.CommunityPartners
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cp => cp.UserId == userId);
                    
                    if (partner != null && partner.Status == ApprovalStatus.Rejected)
                    {
                        logger.LogWarning("Blocking login for rejected user: {UserId}", userId);
                        
                        // Clear any cookie
                        await context.HttpContext.SignOutAsync(AuthenticationConfiguration.CookieScheme);
                        
                        // Redirect to sign-in without error (silent rejection)
                        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                        var frontendOrigin = allowedOrigins.FirstOrDefault() ?? "";
                        var redirectUrl = !string.IsNullOrEmpty(frontendOrigin) 
                            ? $"{frontendOrigin}/sign-in"
                            : "/sign-in";
                        context.Response.Redirect(redirectUrl);
                        context.HandleResponse();
                        return;
                    }
                }
                
                logger.LogInformation("Signing into cookie scheme with principal: {PrincipalIdentity}", context.Principal?.Identity?.Name);
                
                // Explicitly sign into cookie scheme (required when using OnTicketReceived)
                await context.HttpContext.SignInAsync(
                    AuthenticationConfiguration.CookieScheme, 
                    context.Principal!, 
                    context.Properties);
                
                logger.LogInformation("Cookie signed in successfully. Redirecting to frontend callback: {ReturnUrl}", returnUrl);
                
                // Redirect to frontend callback
                context.Response.Redirect(returnUrl);
                context.HandleResponse();
            };

            events.OnAuthenticationFailed = context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.OidcEventHandlers");
                logger.LogError(context.Exception, "Azure B2C OIDC authentication failed");
                
                // Redirect to frontend error page
                var frontendCallbackPath = configuration["AzureB2C:FrontendCallbackPath"] ?? "/auth/callback";
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                var frontendOrigin = allowedOrigins.FirstOrDefault() ?? "";
                var redirectUrl = !string.IsNullOrEmpty(frontendOrigin) 
                    ? $"{frontendOrigin}{frontendCallbackPath}?error=authentication_failed"
                    : $"{frontendCallbackPath}?error=authentication_failed";
                context.Response.Redirect(redirectUrl);
                context.HandleResponse();
                
                return Task.CompletedTask;
            };

            events.OnRemoteFailure = context =>
            {
                var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RICHConnect.Backend.Api.Configuration.Auth.OidcEventHandlers");
                
                // Extract error information from query string (B2C often returns errors in query parameters)
                var errorCode = context.Request.Query["error"].FirstOrDefault();
                var errorDescription = context.Request.Query["error_description"].FirstOrDefault();
                
                // SECURITY: Sanitize error description to avoid logging sensitive data
                var sanitizedErrorDescription = string.IsNullOrEmpty(errorDescription) 
                    ? "No error description in query" 
                    : (errorDescription.Length > 200 ? errorDescription.Substring(0, 200) + "..." : errorDescription);
                
                // Log detailed error information
                logger.LogError(context.Failure, 
                    "Azure B2C OIDC remote failure. Error: {Error}, InnerException: {InnerException}, QueryError: {QueryError}, QueryErrorDescription: {QueryErrorDescription}",
                    context.Failure?.Message,
                    context.Failure?.InnerException?.Message,
                    errorCode ?? "No error code in query",
                    sanitizedErrorDescription);
                
                // Log request details for debugging (SECURITY: redact query string to avoid leaking tokens/codes/state)
                var queryKeys = string.Join(", ", context.Request.Query.Keys);
                logger.LogError("Request details - Path: {Path}, QueryKeys: [{QueryKeys}], Method: {Method}",
                    context.Request.Path,
                    queryKeys,
                    context.Request.Method);
                
                // Redirect to frontend error page with more specific error info if available
                var frontendCallbackPath = configuration["AzureB2C:FrontendCallbackPath"] ?? "/auth/callback";
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                var frontendOrigin = allowedOrigins.FirstOrDefault() ?? "";
                
                // Include error details in redirect if available from query string
                var errorParam = "error=remote_failure";
                if (!string.IsNullOrEmpty(errorCode))
                {
                    errorParam += $"&b2c_error={Uri.EscapeDataString(errorCode)}";
                }
                if (!string.IsNullOrEmpty(errorDescription))
                {
                    errorParam += $"&b2c_error_description={Uri.EscapeDataString(errorDescription)}";
                }
                
                var redirectUrl = !string.IsNullOrEmpty(frontendOrigin) 
                    ? $"{frontendOrigin}{frontendCallbackPath}?{errorParam}"
                    : $"{frontendCallbackPath}?{errorParam}";
                context.Response.Redirect(redirectUrl);
                context.HandleResponse();
                
                return Task.CompletedTask;
            };
            
            return events;
        }

        /// <summary>
        /// Configuration class for special allowed users
        /// </summary>
        private class SpecialAllowedUserConfig
        {
            public string Email { get; set; } = string.Empty;
            public string? Role { get; set; }
        }

        /// <summary>
        /// Parse role string to UserRole enum
        /// </summary>
        private static UserRole ParseRoleFromString(string? roleString)
        {
            if (string.IsNullOrWhiteSpace(roleString))
            {
                return UserRole.FacultySpecialist; // Default
            }

            // Try to parse as enum
            if (Enum.TryParse<UserRole>(roleString, true, out var role))
            {
                return role;
            }

            // Handle common variations
            return roleString.ToLowerInvariant() switch
            {
                "admin" => UserRole.Admin,
                "communitypartner" or "community partner" or "partner" => UserRole.CommunityPartner,
                "richteam" or "rich team" => UserRole.RichTeam,
                "facultyspecialist" or "faculty specialist" or "faculty" => UserRole.FacultySpecialist,
                _ => UserRole.FacultySpecialist // Default fallback
            };
        }
    }
}


