using System.Security.Claims;
using MediatR;
using RICHConnect.Backend.Application.DTOs.Auth;

namespace RICHConnect.Backend.Application.Queries.Auth.GetAuthStatus
{
    /// <summary>
    /// Query to get the authentication status of the current user
    /// </summary>
    public class GetAuthStatusQuery : IRequest<AuthStatusResponseDto>
    {
        /// <summary>
        /// The ClaimsPrincipal of the current user
        /// </summary>
        public ClaimsPrincipal User { get; }

        public GetAuthStatusQuery(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}
