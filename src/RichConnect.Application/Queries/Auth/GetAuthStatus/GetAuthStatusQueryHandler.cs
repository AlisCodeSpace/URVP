using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RICHConnect.Backend.Application.DTOs.Auth;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Application.Queries.Auth.GetAuthStatus
{
    /// <summary>
    /// Handler for GetAuthStatusQuery. Retrieves the authentication status of the current user.
    /// </summary>
    public class GetAuthStatusQueryHandler : IRequestHandler<GetAuthStatusQuery, AuthStatusResponseDto>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GetAuthStatusQueryHandler> _logger;

        public GetAuthStatusQueryHandler(
            AppDbContext context,
            ILogger<GetAuthStatusQueryHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthStatusResponseDto> Handle(GetAuthStatusQuery request, CancellationToken cancellationToken)
        {
            if (!(request.User.Identity?.IsAuthenticated ?? false))
            {
                return new AuthStatusResponseDto { IsAuthenticated = false };
            }

            try
            {
                // Get user ID from claims
                var userIdClaim = request.User.FindFirst("nameid")?.Value ?? request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("User authenticated but no valid user ID found in claims");
                    return new AuthStatusResponseDto 
                    { 
                        IsAuthenticated = false, 
                        Error = "Invalid user ID" 
                    };
                }
                
                // Fetch user data directly from database
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                
                if (user == null)
                {
                    _logger.LogWarning("User ID {UserId} found in claims but not in database", userId);
                    return new AuthStatusResponseDto 
                    { 
                        IsAuthenticated = false, 
                        Error = "User not found" 
                    };
                }
                
                var response = new AuthStatusResponseDto
                {
                    IsAuthenticated = true,
                    UserId = user.Id.ToString(),
                    Email = user.Email,
                    Name = user.Name,
                    Role = (int)user.Role,
                    ProfileImageUrl = user.ProfileImageUrl,
                    RegisteredAt = user.RegisteredAt,
                    AuthenticationScheme = request.User.Identity.AuthenticationType
                };
                
                _logger.LogInformation("Auth status retrieved for user {UserId} with role {Role}", user.Id, user.Role);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving auth status");
                return new AuthStatusResponseDto 
                { 
                    IsAuthenticated = false, 
                    Error = "Internal server error" 
                };
            }
        }
    }
}
