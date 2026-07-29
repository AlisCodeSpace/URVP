using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Configuration.Auth;
using RICHConnect.Backend.Api.Services;
using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Api.Controllers.Auth
{
    /// <summary>
    /// Azure AD SSO endpoints for cookie-based authentication (BFF-style)
    /// </summary>
    [ApiController]
    [Route("api/auth/azuread-sso")]
    public class AzureAdSsoController : ControllerBase
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
        /// Initiates Azure AD OIDC sign-in flow
        /// </summary>
        /// <param name="returnUrl">Frontend URL to redirect to after successful sign-in</param>
        [HttpGet("signin")]
        [AllowAnonymous]
        public IActionResult SignIn(
            [FromQuery, DataType(DataType.Url), StringLength(2048), RegularExpression(@"^https://[^\s]{1,2047}$")]
            string? returnUrl = null)
        {
            try
            {
                // Validate returnUrl against allowlist
                var validatedReturnUrl = _returnUrlValidator.ValidateReturnUrl(returnUrl);
                
                _logger.LogInformation("Initiating Azure AD sign-in. ReturnUrl: {ReturnUrl}", validatedReturnUrl);

                // Store returnUrl in authentication properties to use after callback
                var properties = new AuthenticationProperties
                {
                    RedirectUri = validatedReturnUrl,
                    IsPersistent = true,
                    AllowRefresh = true
                };

                // Trigger OIDC challenge
                return Challenge(properties, AuthenticationConfiguration.AzureAdOidcScheme);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("authorization endpoint") || ex.Message.Contains("configuration"))
            {
                _logger.LogError(ex, "Azure AD OIDC configuration error. Check Authority, ClientId, and metadata endpoint accessibility.");
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
        /// Signs out the user and clears the authentication cookie
        /// </summary>
        /// <param name="returnUrl">Frontend URL to redirect to after sign-out</param>
        [HttpPost("signout")]
        [HttpGet("signout")]
        [AllowAnonymous] // Allow sign-out even if session is expired/invalid
        public async Task<IActionResult> SignOut(
            [FromQuery, DataType(DataType.Url), StringLength(2048), RegularExpression(@"^https://[^\s]{1,2047}$")]
            string? returnUrl = null)
        {
            try
            {
                var validatedReturnUrl = _returnUrlValidator.ValidateReturnUrl(returnUrl);
                
                _logger.LogInformation("Signing out user. ReturnUrl: {ReturnUrl}", validatedReturnUrl);

                // Clear the authentication cookie
                await HttpContext.SignOutAsync(AuthenticationConfiguration.CookieScheme);

                // Optional: Sign out from Azure AD (uncomment if needed)
                // await HttpContext.SignOutAsync(AuthenticationConfiguration.AzureAdOidcScheme);

                if (Request.Headers["Accept"].ToString().Contains("application/json"))
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Sign-out successful",
                        redirectUrl = validatedReturnUrl
                    });
                }

                // Redirect to frontend
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
}


