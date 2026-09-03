using System.Security.Claims;
using FEA.URVP.Api.Services;
using FEA.URVP.Application.Abstractions.Directory;
using FEA.URVP.Application.Commands.Auth.AzureAd;
using MediatR;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace FEA.URVP.Api.Configuration.Auth;

/// <summary>
/// Azure AD OIDC event handlers: user provisioning and claims enrichment.
/// </summary>
/// <remarks>
/// Failure paths redirect to a fixed set of opaque error codes. Identity-provider exception text
/// is logged server-side but never reaches the browser, because it can disclose tenant
/// configuration, internal endpoints and token contents.
/// </remarks>
public static class OidcEventHandlers
{
    private const string LoggerCategory = "FEA.URVP.Api.Configuration.Auth.OidcEventHandlers";

    public static OpenIdConnectEvents CreateAzureAdOidcEvents(IConfiguration configuration)
    {
        var events = new OpenIdConnectEvents();

        events.OnRedirectToIdentityProvider = async context =>
        {
            var logger = GetLogger(context.HttpContext);
            logger.LogDebug("Redirecting to Azure AD authority {Authority}", context.Options.Authority);

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
                    logger.LogWarning("Azure AD token validated but carried no email claim; rejecting sign-in.");
                    context.Fail("Email claim not found in Azure AD token");
                    return;
                }

                var mediator = context.HttpContext.RequestServices.GetRequiredService<IMediator>();
                var directory = context.HttpContext.RequestServices.GetRequiredService<IDirectoryGroupLookup>();
                var safeName = string.IsNullOrWhiteSpace(name) ? email : name;
                var userName = DeriveUserName(preferredUsername, email);
                var affiliation = DeriveAffiliation(context.Principal);

                var directoryGroupRole = directory.ResolveRole(preferredUsername ?? email, email);

                var user = await mediator.Send(
                    new UpsertAzureAdUserCommand(
                        email,
                        safeName,
                        userName,
                        affiliation,
                        directoryGroupRole: directoryGroupRole));

                logger.LogInformation(
                    "Azure AD sign-in provisioned user {UserId} ({Email}) with role {Role}",
                    user.Id,
                    email,
                    user.Role);

                // The principal is rebuilt from database state so the session cookie carries only
                // claims this application controls. Group or role claims asserted by the token
                // are deliberately discarded.
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

                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    context.Principal?.Identity?.AuthenticationType,
                    ClaimTypes.Name,
                    ClaimTypes.Role);

                context.Principal = new ClaimsPrincipal(claimsIdentity);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during Azure AD token validation and user provisioning");
                context.Fail("An error occurred during authentication");
            }
        };

        events.OnAuthenticationFailed = context =>
        {
            GetLogger(context.HttpContext).LogError(
                context.Exception,
                "Azure AD OIDC authentication failed");

            AuthFailureLogger.LogAuthenticationFailed(context.HttpContext, "oidc_token_validation");

            RedirectToFrontendError(context.HttpContext, configuration, "authentication_failed");
            context.HandleResponse();
            return Task.CompletedTask;
        };

        events.OnRemoteFailure = context =>
        {
            var logger = GetLogger(context.HttpContext);

            // A failure to unprotect the state/correlation payload almost always means the data
            // protection key ring changed or the correlation cookie was dropped, which the user
            // can recover from by retrying. It is reported separately so support can distinguish
            // it from a genuine protocol error.
            var isStateProtectionError =
                context.Failure?.Message?.Contains("unprotect", StringComparison.OrdinalIgnoreCase) == true
                || context.Failure?.Message?.Contains("state", StringComparison.OrdinalIgnoreCase) == true;

            logger.LogError(
                context.Failure,
                "Azure AD OIDC remote failure. StateProtectionError: {IsStateProtectionError}",
                isStateProtectionError);

            AuthFailureLogger.LogAuthenticationFailed(context.HttpContext, "oidc_remote_failure");

            RedirectToFrontendError(
                context.HttpContext,
                configuration,
                isStateProtectionError ? "state_protection_failed" : "remote_failure");

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
                FailRedirect(context);
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
                FailRedirect(context);
                return;
            }

            var authorityBase = authority.EndsWith("/v2.0", StringComparison.OrdinalIgnoreCase)
                ? authority[..^5]
                : authority;

            var fallback = $"{authorityBase}/oauth2/v2.0/authorize";
            context.ProtocolMessage.IssuerAddress = fallback;
            logger.LogWarning("Azure AD: Using fallback authorization endpoint {Endpoint}", fallback);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Azure AD: Error loading OpenIdConnect configuration during redirect");
            FailRedirect(context);
        }
    }

    /// <summary>
    /// Aborts the challenge without echoing provider detail. The exception handler is not
    /// involved, so a generic status is set directly.
    /// </summary>
    private static void FailRedirect(RedirectContext context)
    {
        context.HandleResponse();
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
    }

    private static void RedirectToFrontendError(
        HttpContext httpContext,
        IConfiguration configuration,
        string errorCode)
    {
        var callbackPath = configuration["AzureAd:FrontendCallbackPath"] ?? "/auth/callback";
        var validator = httpContext.RequestServices.GetRequiredService<ReturnUrlValidationService>();

        var target = validator.BuildFrontendUrl(
            $"{callbackPath}?error={Uri.EscapeDataString(errorCode)}");

        httpContext.Response.Redirect(target);
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
            .CreateLogger(LoggerCategory);
}
