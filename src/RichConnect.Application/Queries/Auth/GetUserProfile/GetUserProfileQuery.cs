using System.Security.Claims;
using MediatR;
using RICHConnect.Backend.Application.DTOs.Auth;

namespace RICHConnect.Backend.Application.Queries.Auth.GetUserProfile
{
    /// <summary>
    /// Query to get the profile information of the current authenticated user
    /// </summary>
    public class GetUserProfileQuery : IRequest<UserProfileResponseDto>
    {
        /// <summary>
        /// The ClaimsPrincipal of the current user
        /// </summary>
        public ClaimsPrincipal User { get; }

        public GetUserProfileQuery(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}
