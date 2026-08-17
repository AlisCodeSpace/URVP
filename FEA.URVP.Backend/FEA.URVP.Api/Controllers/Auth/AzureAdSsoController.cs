using System.ComponentModel.DataAnnotations;
using FEA.URVP.Api.Configuration.Auth;
using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FEA.URVP.Api.Controllers.Auth;

/// <summary>
/// Azure AD (AUB SSO) endpoints for cookie-based authentication (BFF-style).
/// </summary>
[ApiController]
[Route("api/auth/azuread-sso")]
public sealed class AzureAdSsoController : ApiControllerBase
{
    private readonly ReturnUrlValidationService _returnUrlValidator;
    private readonly ILogger<AzureAdSsoController> _logger;

    public AzureAdSsoController(
        ReturnUrlValidationService returnUrlValidator,
        ILogger<AzureAdSsoController> logger)
    {
        _returnUrlValidator = returnUrlValidator;
        _logger = logger;
    }

    /// <summary>
    /// Initiates Azure AD OIDC sign-in.
    /// </summary>
    [HttpGet("signin")]
    [AllowAnonymous]
    public IActionResult SignIn(
        [FromQuery, StringLength(2048)]
        string? returnUrl = null)
    {
        try
        {
            var validatedReturnUrl = _returnUrlValidator.ValidateReturnUrl(returnUrl);

            _logger.LogInformation("Initiating Azure AD sign-in. ReturnUrl: {ReturnUrl}", validatedReturnUrl);

            var properties = new AuthenticationProperties
            {
                RedirectUri = validatedReturnUrl,
                IsPersistent = true,
                AllowRefresh = true
            };

            return Challenge(properties, AuthenticationConfiguration.AzureAdOidcScheme);
        }
        catch (InvalidOperationException ex) when (
            ex.Message.Contains("authorization endpoint", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("configuration", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError(ex, "Azure AD OIDC configuration error");
            return BadRequest(new
            {
                success = false,
                message = "Cannot redirect to the authorization endpoint, the configuration may be missing or invalid.",
                errors = new[] { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating Azure AD sign-in");
            return BadRequest(new
            {
                success = false,
                message = "Failed to initiate sign-in",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Signs out and clears the authentication cookie.
    /// </summary>
    [HttpPost("signout")]
    [HttpGet("signout")]
    [AllowAnonymous]
    public async Task<IActionResult> SignOut(
        [FromQuery, StringLength(2048)]
        string? returnUrl = null)
    {
        try
        {
            var validatedReturnUrl = _returnUrlValidator.ValidateReturnUrl(returnUrl);

            _logger.LogInformation("Signing out user. ReturnUrl: {ReturnUrl}", validatedReturnUrl);

            await HttpContext.SignOutAsync(AuthenticationConfiguration.CookieScheme);

            if (Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(new
                {
                    success = true,
                    message = "Sign-out successful",
                    redirectUrl = validatedReturnUrl
                });
            }

            return Redirect(validatedReturnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign-out");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during sign-out"
            });
        }
    }
}
