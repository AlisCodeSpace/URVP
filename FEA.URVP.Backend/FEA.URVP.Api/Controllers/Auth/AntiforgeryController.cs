using FEA.URVP.Api.Configuration.Security;
using FEA.URVP.Api.Controllers.Base;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FEA.URVP.Api.Controllers.Auth;

/// <summary>
/// Issues the antiforgery request token the frontend echoes back in
/// <see cref="AntiforgeryConfiguration.HeaderName"/> on every mutating call.
/// </summary>
/// <remarks>
/// Anonymous so the token can be obtained before sign-in. The token alone confers nothing: it is
/// only accepted together with the paired HttpOnly cookie, which a cross-site attacker cannot
/// read or set.
/// </remarks>
[ApiController]
[Route("api/auth")]
public sealed class AntiforgeryController : ApiControllerBase
{
    private readonly IAntiforgery _antiforgery;

    public AntiforgeryController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    [HttpGet("csrf")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingConfiguration.AuthPolicy)]
    public IActionResult GetToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        return SuccessResponse(
            new AntiforgeryTokenResponse(AntiforgeryConfiguration.HeaderName, tokens.RequestToken ?? string.Empty),
            "Antiforgery token issued");
    }
}

public sealed record AntiforgeryTokenResponse(string HeaderName, string Token);
