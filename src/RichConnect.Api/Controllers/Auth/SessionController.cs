using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using RICHConnect.Backend.Application.Queries.Auth.GetAuthStatus;
using RICHConnect.Backend.Application.Queries.Auth.GetUserProfile;

namespace RICHConnect.Backend.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthStatusController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthStatusController> _logger;
        
        public AuthStatusController(IMediator mediator, ILogger<AuthStatusController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet("status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAuthStatus()
        {
            var query = new GetAuthStatusQuery(User);
            var result = await _mediator.Send(query);
            
            if (!result.IsAuthenticated && !string.IsNullOrEmpty(result.Error))
            {
                if (result.Error == "Internal server error")
                {
                    return StatusCode(500, new { isAuthenticated = false, error = result.Error });
                }
                return Ok(new { isAuthenticated = false, error = result.Error });
            }
            
            return Ok(new 
            { 
                isAuthenticated = result.IsAuthenticated,
                userId = result.UserId,
                email = result.Email,
                name = result.Name,
                role = result.Role,
                profileImageUrl = result.ProfileImageUrl,
                registeredAt = result.RegisteredAt,
                authenticationScheme = result.AuthenticationScheme
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            var query = new GetUserProfileQuery(User);
            var result = await _mediator.Send(query);
            
            if (!string.IsNullOrEmpty(result.Error))
            {
                if (result.Error == "Invalid user ID")
                {
                    return BadRequest(new { error = result.Error });
                }
                else if (result.Error == "User not found")
                {
                    return NotFound(new { error = result.Error });
                }
                else if (result.Error == "Internal server error")
                {
                    return StatusCode(500, new { error = result.Error });
                }
                return BadRequest(new { error = result.Error });
            }
            
            return Ok(new 
            { 
                userId = result.UserId,
                email = result.Email,
                name = result.Name,
                role = result.Role,
                profileImageUrl = result.ProfileImageUrl,
                registeredAt = result.RegisteredAt,
                createdAt = result.CreatedAt,
                updatedAt = result.UpdatedAt,
                authenticationScheme = result.AuthenticationScheme,
                hasProfile = result.HasProfile,
                profileStatus = result.ProfileStatus
            });
        }
    }
}