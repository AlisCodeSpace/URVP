using System.Security.Claims;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using RICHConnect.Backend.Infrastructure.Events;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Application.Services.FMIS;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Handlers.Auth
{
    /// <summary>
    /// OBSOLETE — not wired into the authentication pipeline.
    /// The active Azure AD OIDC event handlers are in
    /// <see cref="RICHConnect.Backend.Api.Configuration.Auth.OidcEventHandlers.CreateAzureAdOidcEvents"/>.
    /// Do not register this class; it lacks the FMIS role-override and claim-replacement logic.
    /// </summary>
    [Obsolete("Not used. See OidcEventHandlers.CreateAzureAdOidcEvents in the Api project.")]
    public class AzureAdOidcEventHandlers
    {
        private readonly ILogger<AzureAdOidcEventHandlers> _logger;

        public AzureAdOidcEventHandlers(ILogger<AzureAdOidcEventHandlers> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handles token validation event - creates/updates user and adds custom claims
        /// </summary>
        public async Task OnTokenValidated(TokenValidatedContext context)
        {
            _logger.LogInformation("🔥 OIDC EVENT: OnTokenValidated triggered");

            // Log all claims from the principal
            _logger.LogInformation("🔍 OIDC Principal claims:");
            foreach (var claim in context.Principal?.Claims ?? Enumerable.Empty<Claim>())
            {
                _logger.LogInformation("  - {ClaimType}: {ClaimValue}", claim.Type, claim.Value);
            }

            try
            {
                _logger.LogInformation("🔄 Starting user creation process...");

                // Extract user information from claims
                var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value
                    ?? context.Principal?.FindFirst("preferred_username")?.Value
                    ?? context.Principal?.FindFirst("email")?.Value;

                // Extract name from multiple possible claim sources
                var name = context.Principal?.FindFirst("name")?.Value
                    ?? context.Principal?.FindFirst(ClaimTypes.Name)?.Value;

                // If no name found, construct from given name + surname
                if (string.IsNullOrEmpty(name))
                {
                    var givenName = context.Principal?.FindFirst("http://schemas.microsoft.com/ws/2005/05/identity/claims/givenname")?.Value;
                    var surname = context.Principal?.FindFirst("http://schemas.microsoft.com/ws/2005/05/identity/claims/surname")?.Value;

                    if (!string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(surname))
                    {
                        name = $"{givenName} {surname}";
                    }
                    else if (!string.IsNullOrEmpty(givenName))
                    {
                        name = givenName;
                    }
                    else if (!string.IsNullOrEmpty(surname))
                    {
                        name = surname;
                    }
                }

                _logger.LogInformation("🔍 Extracted from OIDC - Email: {Email}, Name: {Name}", email, name);

                if (!string.IsNullOrEmpty(email))
                {
                    _logger.LogInformation("✅ Email found, proceeding with user creation...");

                    // Create a new scope for services
                    using var scope = context.HttpContext.RequestServices.CreateScope();
                    
                    // Generate a correlation ID for tracking this authentication flow
                    var correlationId = Guid.NewGuid().ToString("N");
                    _logger.LogInformation("🔄 Authentication flow correlation ID: {CorrelationId}", correlationId);
                    
                    // Use MediatR to send the command
                    _logger.LogInformation("🔄 Using CQRS for user creation/update [CorrelationId: {CorrelationId}]", correlationId);
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    
                    // Use UpsertAzureAdUserCommand to create or update the user
                    var user = await mediator.Send(new RICHConnect.Backend.Application.Commands.Auth.AzureAd.UpsertAzureAdUserCommand(
                        email,
                        name ?? email.Split('@')[0]
                    ));
                    
                    _logger.LogInformation("✅ User processed via CQRS with ID: {UserId} and role: {Role} [CorrelationId: {CorrelationId}]", 
                        user.Id, user.Role, correlationId);
                        
                    // Get the event bus to publish authentication events
                    var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
                    
                    // Publish UserAuthenticatedEvent for the OIDC authentication
                    await eventBus.PublishAsync(new UserAuthenticatedEvent(
                        user.Id,
                        user.Email,
                        "AzureAd",
                        "OIDC",
                        true, // New session via OIDC
                        correlationId
                    ));
                    
                    _logger.LogInformation("✅ Published UserAuthenticatedEvent for user {UserId} [CorrelationId: {CorrelationId}]", 
                        user.Id, correlationId);

                    // Add custom claims to the principal (same for both paths)
                    var claims = new List<Claim>
                    {
                        new Claim("nameid", user.Id.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role.ToString()),
                        new Claim("rc_role", ((int)user.Role).ToString()) // Numeric DB role for frontend
                    };

                    var identity = context.Principal?.Identity as ClaimsIdentity;
                    identity?.AddClaims(claims);
                    _logger.LogInformation("✅ Custom claims added to principal: nameid={NameId}, role={Role}", user.Id, user.Role);
                }
                else
                {
                    _logger.LogWarning("❌ No email found in OIDC claims");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error creating user during OIDC token validation");
                // Don't throw here - let the authentication continue even if user creation fails
            }
        }

        /// <summary>
        /// Handles redirect to identity provider event - stores return URL
        /// </summary>
        public Task OnRedirectToIdentityProvider(RedirectContext context)
        {
            _logger.LogInformation("🔥 OIDC EVENT: OnRedirectToIdentityProvider triggered");
            _logger.LogInformation("🔍 Redirect URI: {RedirectUri}", context.Properties?.RedirectUri);
            
            // Force account picker to avoid silent re-login with cached session
            context.ProtocolMessage.Prompt = "select_account";

            // Store the return URL for later use
            if (context.Properties?.RedirectUri != null)
            {
                context.Properties.Items["returnUrl"] = context.Properties.RedirectUri;
                _logger.LogInformation("✅ Return URL stored: {ReturnUrl}", context.Properties.RedirectUri);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles redirect to identity provider for sign out event
        /// </summary>
        public Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            _logger.LogInformation("🔥 OIDC EVENT: OnRedirectToIdentityProviderForSignOut triggered");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles token response received event
        /// </summary>
        public Task OnTokenResponseReceived(TokenResponseReceivedContext context)
        {
            _logger.LogInformation("🔥 OIDC EVENT: OnTokenResponseReceived triggered");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles ticket received event - signs in user and redirects to frontend
        /// </summary>
        public async Task OnTicketReceived(TicketReceivedContext context)
        {
            _logger.LogInformation("🔥 OIDC EVENT: OnTicketReceived triggered - Authentication successful!");
            
            // Create a scope for services (needed for configuration and FMIS check)
            using var scope = context.HttpContext.RequestServices.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            
            // Get the stored return URL
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            var defaultReturnUrl = allowedOrigins.FirstOrDefault() ?? "/";
            var returnUrl = context.Properties?.Items["returnUrl"] ?? defaultReturnUrl;
            _logger.LogInformation("🔍 Redirecting to: {ReturnUrl}", returnUrl);
            
            // Get user information from the authenticated principal
            var userEmail = context.Principal?.FindFirst(ClaimTypes.Email)?.Value
                ?? context.Principal?.FindFirst("preferred_username")?.Value
                ?? context.Principal?.FindFirst("email")?.Value;
            var userName = context.Principal?.FindFirst("name")?.Value 
                ?? context.Principal?.FindFirst(ClaimTypes.Name)?.Value;
            var userId = context.Principal?.FindFirst("nameid")?.Value;
            var userRole = context.Principal?.FindFirst("rc_role")?.Value 
                ?? context.Principal?.FindAll(ClaimTypes.Role).LastOrDefault()?.Value;
            
            // Check FMIS membership if enforcement is enabled
            var enforceForAzureAd = configuration.GetValue<bool>("Fmis:EnforceForAzureAdLogins", false);
            
            if (enforceForAzureAd)
            {
                if (string.IsNullOrWhiteSpace(userEmail))
                {
                    _logger.LogWarning("[FMIS] AUB SSO login blocked - no email in claims");
                    context.HandleResponse();
                    context.Response.Redirect($"{returnUrl.TrimEnd('/')}/oauth/error?reason=no-email");
                    return;
                }
                
                var fmisChecker = scope.ServiceProvider.GetRequiredService<IFmisMembershipChecker>();
                var isAllowed = await fmisChecker.IsEmailAllowedAsync(userEmail);
                
                if (!isAllowed)
                {
                    _logger.LogWarning("[FMIS] AUB SSO login blocked for email: {Email} - not in FMIS or exception list", userEmail);
                    context.HandleResponse();
                    context.Response.Redirect($"{returnUrl.TrimEnd('/')}/oauth/error?reason=not-in-fmis&email={Uri.EscapeDataString(userEmail)}");
                    return;
                }
                
                _logger.LogInformation("[FMIS] AUB SSO login allowed for email: {Email}", userEmail);
            }
            
            // Create user info object and encode it for URL parameter
            // Convert role from string to numeric value for frontend compatibility
            int roleValue = 3; // Default to Faculty Specialist (3)
            if (int.TryParse(userRole, out int parsedRole))
            {
                roleValue = parsedRole;
            }
            else if (userRole == "Faculty Specialist")
            {
                roleValue = 3; // Faculty Specialist
            }
            else if (userRole == "CommunityPartner")
            {
                roleValue = 1; // Community Partner
            }
            else if (userRole == "Admin")
            {
                roleValue = 0; // Admin
            }
            
            var userInfo = new
            {
                id = userId,
                email = userEmail,
                name = userName,
                role = roleValue
            };
            
            var userInfoJson = JsonSerializer.Serialize(userInfo);
            var userInfoEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(userInfoJson));
            
            // Sign the user in with the cookie scheme before redirecting
            await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, context.Principal!, context.Properties);
            
            // Publish UserLoggedInEvent
            if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var parsedUserId))
            {
                try
                {
                    // Create a new scope for services
                    using var eventScope = context.HttpContext.RequestServices.CreateScope();
                    var eventBus = eventScope.ServiceProvider.GetRequiredService<IEventBus>();
                    var eventCorrelationId = Guid.NewGuid().ToString("N");
                    
                    // Publish UserLoggedInEvent
                    await eventBus.PublishAsync(new UserLoggedInEvent(
                        parsedUserId,
                        userEmail ?? "unknown@email.com",
                        userName ?? "Unknown User",
                        roleValue == 3 ? UserRole.FacultySpecialist : 
                            roleValue == 0 ? UserRole.Admin : 
                            roleValue == 1 ? UserRole.CommunityPartner : 
                            roleValue == 2 ? UserRole.RichTeam : UserRole.FacultySpecialist,
                        "AzureAd",
                        eventCorrelationId
                    ));
                    
                    _logger.LogInformation("✅ Published UserLoggedInEvent for user {UserId} [CorrelationId: {CorrelationId}]", 
                        parsedUserId, eventCorrelationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "💥 Error publishing UserLoggedInEvent for user {UserId}", userId);
                    // Don't throw - we still want to redirect the user
                }
            }
            
            // Redirect to frontend callback with user info
            var callbackUrl = $"{returnUrl.TrimEnd('/')}/oauth/aub-callback?userInfo={userInfoEncoded}";
            _logger.LogInformation("✅ Redirecting to frontend callback: {CallbackUrl}", callbackUrl);
            
            context.HandleResponse();
            context.Response.Redirect(callbackUrl);
        }
    }
}
