using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FEA.URVP.Api.Configuration.Auth;
using FEA.URVP.Api.Controllers.Base;
using FEA.URVP.Api.Services;
using FEA.URVP.Application.Commands.Auth.AzureAd;
using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Domain.Catalog;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FEA.URVP.Api.Controllers.Auth;

/// <summary>
/// Email sign-in (no password) for demo accounts.
/// </summary>
/// <remarks>
/// Never reachable in Production. See <see cref="DevSignInPolicy"/> for why this cannot be
/// re-enabled by configuration.
/// </remarks>
[ApiController]
[Route("api/auth/dev")]
public sealed class DevAuthController : ApiControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly IMediator _mediator;
    private readonly ReturnUrlValidationService _returnUrlValidator;
    private readonly ILogger<DevAuthController> _logger;

    public DevAuthController(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IMediator mediator,
        ReturnUrlValidationService returnUrlValidator,
        ILogger<DevAuthController> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _mediator = mediator;
        _returnUrlValidator = returnUrlValidator;
        _logger = logger;
    }

    /// <summary>
    /// Signs in as a whitelisted demo user by email and redirects to returnUrl.
    /// </summary>
    [HttpGet("signin")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingConfiguration.AuthPolicy)]
    public async Task<IActionResult> SignIn(
        [FromQuery, Required, EmailAddress, StringLength(256)] string email,
        [FromQuery, StringLength(2048)]
        string? returnUrl = null)
    {
        if (!DevSignInPolicy.IsEnabled(_configuration, _environment))
        {
            return NotFound();
        }

        if (!DevAuthAccounts.TryGet(email, out var account) || account is null)
        {
            _logger.LogWarning("Dev sign-in rejected for unknown email: {Email}", email);
            return BadRequest(new
            {
                success = false,
                message = "Unknown development account. Use faculty@urvp.com, student@urvp.com, or admin@urvp.com."
            });
        }

        var validatedReturnUrl = _returnUrlValidator.ValidateReturnUrl(returnUrl);

        _logger.LogInformation(
            "Dev email sign-in for {Email} ({Role}). ReturnUrl: {ReturnUrl}",
            account.Email,
            account.Role,
            validatedReturnUrl);

        var user = await _mediator.Send(new UpsertAzureAdUserCommand(
            account.Email,
            account.Name,
            account.UserName,
            DevAuthAccounts.Affiliation,
            profileImageUrl: null,
            roleOverride: account.Role));

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

        var identity = new ClaimsIdentity(
            claims,
            AuthenticationConfiguration.CookieScheme,
            ClaimTypes.Name,
            ClaimTypes.Role);

        await HttpContext.SignInAsync(
            AuthenticationConfiguration.CookieScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                RedirectUri = validatedReturnUrl
            });

        return Redirect(validatedReturnUrl);
    }
}
