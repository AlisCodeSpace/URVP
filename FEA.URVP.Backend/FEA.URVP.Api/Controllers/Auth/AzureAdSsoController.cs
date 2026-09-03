using System.ComponentModel.DataAnnotations;
using FEA.URVP.Api.Configuration.Auth;
using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FEA.URVP.Api.Controllers.Auth;

/// <summary>
/// Azure AD (AUB SSO) sign-in and sign-out for the cookie session.
/// </summary>
/// <remarks>
/// Exceptions are deliberately not caught here. The global exception handler owns the response
/// shape and is the only component allowed to decide what detail an environment may disclose;
/// catching locally previously echoed raw exception messages to the browser.
/// </remarks>
[ApiController]
[Route("api/auth/azuread-sso")]
[EnableRateLimiting(RateLimitingConfiguration.AuthPolicy)]
public sealed class AzureAdSsoController : ApiControllerBase
{
    private readonly ReturnUrlValidationService _returnUrlValidator;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly ILogger<AzureAdSsoController> _logger;

    public AzureAdSsoController(
        ReturnUrlValidationService returnUrlValidator,
        IAuthenticationSchemeProvider schemeProvider,
        ILogger<AzureAdSsoController> logger)
    {
        _returnUrlValidator = returnUrlValidator;
        _schemeProvider = schemeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Initiates Azure AD OIDC sign-in. This is the only place that challenges the OIDC scheme;
    /// everything else challenges the cookie scheme and returns 401.
    /// </summary>
    [HttpGet("signin")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(
        [FromQuery, StringLength(2048)] string? returnUrl = null)
    {
        var validatedReturnUrl = _returnUrlValidator.ValidateReturnUrl(returnUrl);

        // The scheme is absent only on a developer machine with no tenant settings; the startup
        // validation refuses to boot without them anywhere else. Answering 503 keeps that case a
        // legible configuration problem instead of an unhandled exception.
        if (await _schemeProvider.GetSchemeAsync(AuthenticationConfiguration.AzureAdOidcScheme) is null)
        {
            _logger.LogError("Azure AD sign-in was requested but the OIDC scheme is not registered.");
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                Contracts.ApiResponse<object>.ErrorResult("Single sign-on is not available."));
        }

        _logger.LogInformation("Initiating Azure AD sign-in.");

        var properties = new AuthenticationProperties
        {
            RedirectUri = validatedReturnUrl,
            IsPersistent = true,
            AllowRefresh = true
        };

        return Challenge(properties, AuthenticationConfiguration.AzureAdOidcScheme);
    }

    /// <summary>
    /// Clears the application session cookie.
    /// </summary>
    [HttpPost("signout")]
    [HttpGet("signout")]
    [AllowAnonymous]
    public async Task<IActionResult> SignOut(
        [FromQuery, StringLength(2048)] string? returnUrl = null)
    {
        var validatedReturnUrl = _returnUrlValidator.ValidateReturnUrl(returnUrl);

        await HttpContext.SignOutAsync(AuthenticationConfiguration.CookieScheme);

        _logger.LogInformation("Session cookie cleared on sign-out.");

        if (Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            return SuccessResponse(
                new SignOutResponse(validatedReturnUrl),
                "Sign-out successful");
        }

        return Redirect(validatedReturnUrl);
    }
}

public sealed record SignOutResponse(string RedirectUrl);
