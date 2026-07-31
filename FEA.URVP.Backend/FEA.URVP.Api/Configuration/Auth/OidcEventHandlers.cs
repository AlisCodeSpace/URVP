using System.Security.Claims;
using FEA.URVP.Application.Commands.Auth.AzureAd;
using MediatR;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace FEA.URVP.Api.Configuration.Auth;

/// <summary>
/// Azure AD OIDC event handlers: user provisioning and claims enrichment.
/// </summary>
public static class OidcEventHandlers
{
    public static OpenIdConnectEvents CreateAzureAdOidcEvents(IConfiguration configuration)
    {
        var events = new OpenIdConnectEvents();

        events.OnRedirectToIdentityProvider = async context =>
        {
            var logger = GetLogger(context.HttpContext);
            logger.LogInformation(
                "Redirecting to Azure AD. Authority: {Authority}, ClientId: {ClientId}",
                context.Options.Authority,
                context.Options.ClientId);

            await EnsureAuthorizationEndpointAsync(context, logger);
        };

        events.OnTokenValidated = async context =>
        {
            var logger = GetLogger(context.HttpContext);

            try
            {
                var preferredUsername = context.Principal?.FindFirst("preferred_username")?.Value;
                var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value
                    ?? preferredUsername
                    ?? context.Principal?.FindFirst("email")?.Value
                    ?? context.Principal?.FindFirst("upn")?.Value;

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

                var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
                var safeName = string.IsNullOrWhiteSpace(name) ? email : name;
                var userName = DeriveUserName(preferredUsername, email);
                var affiliation = DeriveAffiliation(context.Principal);
                var user = await mediator.Send(
                    new UpsertAzureAdUserCommand(email, safeName, userName, affiliation));

                logger.LogInformation("User provisioned: {UserId}, Role: {Role}", user.Id, user.Role);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Name, user.Name),
                    new(ClaimTypes.Role, user.Role.ToString()),
                    new("userId", user.Id.ToString()),
                    new("role", user.Role.ToString()),
                    new("numericRole", ((int)user.Role).ToString())
                };

                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    claims.Add(new Claim("profileImageUrl", user.ProfileImageUrl));
                }

                var claimsIdentity = new ClaimsIdentity(claims, context.Principal?.Identity?.AuthenticationType);
                context.Principal = new ClaimsPrincipal(claimsIdentity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during Azure AD OIDC token validation and user provisioning");
                context.Fail("An error occurred during authentication");
            }
        };

        events.OnAuthenticationFailed = async context =>
        {
            var logger = GetLogger(context.HttpContext);
            logger.LogError(
                context.Exception,
                "Azure AD OIDC authentication failed: {Message}",
                context.Exception?.Message);

            RedirectToFrontendError(context.Response, configuration, "authentication_failed");
            context.HandleResponse();
            await Task.CompletedTask;
        };

        events.OnRemoteFailure = context =>
        {
            var logger = GetLogger(context.HttpContext);
            var isStateProtectionError =
                context.Failure?.Message?.Contains("unprotect", StringComparison.OrdinalIgnoreCase) == true
                || context.Failure?.Message?.Contains("State", StringComparison.OrdinalIgnoreCase) == true;

            logger.LogError(
                context.Failure,
                "Azure AD OIDC remote failure. StateProtectionError: {IsStateProtectionError}",
                isStateProtectionError);

            var errorCode = isStateProtectionError ? "state_protection_failed" : "remote_failure";
            RedirectToFrontendError(context.Response, configuration, errorCode);
            context.HandleResponse();
            return Task.CompletedTask;
        };

        return events;
    }

    private static async Task EnsureAuthorizationEndpointAsync(RedirectContext context, ILogger logger)
    {
        try
        {
            var configurationManager = context.Options.ConfigurationManager;
            if (configurationManager is null)
            {
                logger.LogWarning("Azure AD: ConfigurationManager is null. Metadata discovery may fail.");
                return;
            }

            var config = await configurationManager.GetConfigurationAsync(context.HttpContext.RequestAborted);
            if (config is null)
            {
                logger.LogError("Azure AD: OpenIdConnect configuration is null.");
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            if (!string.IsNullOrEmpty(config.AuthorizationEndpoint))
            {
                return;
            }

            var authority = context.Options.Authority?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(authority))
            {
                logger.LogError("Azure AD: Cannot compute fallback authorize endpoint; Authority is empty.");
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            var authorityBase = authority.EndsWith("/v2.0", StringComparison.OrdinalIgnoreCase)
                ? authority[..^5]
                : authority;

            var fallback = $"{authorityBase}/oauth2/v2.0/authorize";
            context.ProtocolMessage.IssuerAddress = fallback;
            logger.LogWarning("Azure AD: Using fallback authorization endpoint: {Endpoint}", fallback);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure AD: Error loading OpenIdConnect configuration during redirect");
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }

    private static void RedirectToFrontendError(
        HttpResponse response,
        IConfiguration configuration,
        string errorCode)
    {
        var frontendCallbackPath = configuration["AzureAd:FrontendCallbackPath"] ?? "/auth/callback";
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        var frontendOrigin = allowedOrigins.FirstOrDefault() ?? string.Empty;
        var redirectUrl = !string.IsNullOrEmpty(frontendOrigin)
            ? $"{frontendOrigin}{frontendCallbackPath}?error={errorCode}"
            : $"{frontendCallbackPath}?error={errorCode}";

        response.Redirect(redirectUrl);
    }

    private static string DeriveUserName(string? preferredUsername, string email)
    {
        var source = !string.IsNullOrWhiteSpace(preferredUsername) ? preferredUsername : email;
        var at = source.IndexOf('@');
        return (at > 0 ? source[..at] : source).Trim();
    }

    private static string DeriveAffiliation(ClaimsPrincipal? principal)
    {
        var affiliation =
            principal?.FindFirst("department")?.Value
            ?? principal?.FindFirst("companyName")?.Value
            ?? principal?.FindFirst("organization")?.Value
            ?? principal?.FindFirst("extn.OrganizationName")?.Value;

        return string.IsNullOrWhiteSpace(affiliation)
            ? "American University of Beirut"
            : affiliation.Trim();
    }

    private static ILogger GetLogger(HttpContext httpContext) =>
        httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("FEA.URVP.Api.Configuration.Auth.OidcEventHandlers");
}
