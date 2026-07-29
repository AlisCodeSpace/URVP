using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Configuration.Auth;
using RICHConnect.Backend.Api.Services;
using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Api.Controllers.Auth
{
    /// <summary>
    /// Azure B2C SSO endpoints for cookie-based authentication (BFF-style)
    /// </summary>
    [ApiController]
    [Route("api/auth/azureb2c-sso")]
    public class AzureB2CSsoController : ControllerBase
    {
        private readonly ReturnUrlValidationService _returnUrlValidator;
        private readonly ILogger<AzureB2CSsoController> _logger;

        public AzureB2CSsoController(
            ReturnUrlValidationService returnUrlValidator,
            ILogger<AzureB2CSsoController> logger)
        {
            _returnUrlValidator = returnUrlValidator;
            _logger = logger;
        }

        /// <summary>
        /// Initiates Azure B2C OIDC sign-in flow
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
                
                _logger.LogInformation("Initiating Azure B2C sign-in. ReturnUrl: {ReturnUrl}", validatedReturnUrl);

                // Store returnUrl in authentication properties to use after callback
                var properties = new AuthenticationProperties
                {
                    RedirectUri = validatedReturnUrl,
                    IsPersistent = true,
                    AllowRefresh = true
                };

                // Trigger OIDC challenge
                return Challenge(properties, AuthenticationConfiguration.AzureB2COidcScheme);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating Azure B2C sign-in");
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to initiate sign-in"
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

                // Optional: Sign out from Azure B2C (uncomment if needed)
                // await HttpContext.SignOutAsync(AuthenticationConfiguration.AzureB2COidcScheme);

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
