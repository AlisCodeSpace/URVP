using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RICHConnect.Backend.Application.DTOs.Auth;
using RICHConnect.Backend.Infrastructure.Data;
using RICHConnect.Backend.Domain.Enums;
using RICHConnect.Backend.Domain.Entities.Partners;

namespace RICHConnect.Backend.Application.Queries.Auth.GetUserProfile
{
    /// <summary>
    /// Handler for GetUserProfileQuery. Retrieves the profile information of the current authenticated user.
    /// </summary>
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileResponseDto>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GetUserProfileQueryHandler> _logger;

        public GetUserProfileQueryHandler(
            AppDbContext context,
            ILogger<GetUserProfileQueryHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserProfileResponseDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = request.User.FindFirst("nameid")?.Value ?? request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Authorized user has no valid user ID in claims");
                    return new UserProfileResponseDto { Error = "Invalid user ID" };
                }
                
                // Fetch user data directly from database with related profile data
                var user = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.FacultySpecialist)
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                
                if (user == null)
                {
                    _logger.LogWarning("Authorized user ID {UserId} not found in database", userId);
                    return new UserProfileResponseDto { Error = "User not found" };
                }
                
                // Determine profile status based on user role
                bool hasProfile = false;
                int? profileStatus = null;
                
                if (user.Role == UserRole.FacultySpecialist)
                {
                    // Faculty Specialist uses FacultySpecialist table
                    hasProfile = user.FacultySpecialist != null;
                    // Note: Status field has been removed from FacultySpecialist
                    profileStatus = null;
                }
                else if (user.Role == UserRole.CommunityPartner)
                {
                    // Community Partner uses CommunityPartner table
                    var partnerProfile = await _context.CommunityPartners
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cp => cp.UserId == userId, cancellationToken);
                    
                    hasProfile = partnerProfile != null;
                    profileStatus = partnerProfile?.Status != null ? (int)partnerProfile.Status : null;
                }
                
                var response = new UserProfileResponseDto
                {
                    UserId = user.Id.ToString(),
                    Email = user.Email,
                    Name = user.Name,
                    Role = (int)user.Role,
                    ProfileImageUrl = user.ProfileImageUrl,
                    RegisteredAt = user.RegisteredAt,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    AuthenticationScheme = request.User.Identity?.AuthenticationType,
                    HasProfile = hasProfile,
                    ProfileStatus = profileStatus
                };
                
                _logger.LogInformation("Profile retrieved for user {UserId}", user.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return new UserProfileResponseDto { Error = "Internal server error" };
            }
        }
    }
}
