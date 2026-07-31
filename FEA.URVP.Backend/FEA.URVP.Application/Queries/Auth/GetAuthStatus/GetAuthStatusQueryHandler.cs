using System.Security.Claims;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Application.Queries.Auth.GetAuthStatus;

public sealed class GetAuthStatusQueryHandler : IRequestHandler<GetAuthStatusQuery, AuthStatusResponseDto>
{
    private readonly IUserRepository _users;
    private readonly ILogger<GetAuthStatusQueryHandler> _logger;

    public GetAuthStatusQueryHandler(
        IUserRepository users,
        ILogger<GetAuthStatusQueryHandler> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<AuthStatusResponseDto> Handle(
        GetAuthStatusQuery request,
        CancellationToken cancellationToken)
    {
        if (!(request.User.Identity?.IsAuthenticated ?? false))
        {
            return new AuthStatusResponseDto { IsAuthenticated = false };
        }

        try
        {
            var userIdClaim = request.User.FindFirst("userId")?.Value
                ?? request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Authenticated principal has no valid user ID claim");
                return new AuthStatusResponseDto
                {
                    IsAuthenticated = false,
                    Error = "Invalid user ID"
                };
            }

            var user = await _users.FindByIdAsync(userId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User ID {UserId} in claims was not found in the database", userId);
                return new AuthStatusResponseDto
                {
                    IsAuthenticated = false,
                    Error = "User not found"
                };
            }

            return new AuthStatusResponseDto
            {
                IsAuthenticated = true,
                UserId = user.Id.ToString(),
                Email = user.Email,
                Name = user.Name,
                UserName = user.UserName,
                Affiliation = user.Affiliation,
                Role = (int)user.Role,
                ProfileImageUrl = user.ProfileImageUrl,
                RegisteredAt = user.RegisteredAt,
                AuthenticationScheme = request.User.Identity.AuthenticationType
            };
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
