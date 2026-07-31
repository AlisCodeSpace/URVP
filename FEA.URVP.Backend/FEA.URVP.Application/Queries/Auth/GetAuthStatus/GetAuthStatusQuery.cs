using System.Security.Claims;
using FEA.URVP.Application.DTOs.Auth;
using MediatR;

namespace FEA.URVP.Application.Queries.Auth.GetAuthStatus;

public sealed class GetAuthStatusQuery : IRequest<AuthStatusResponseDto>
{
    public ClaimsPrincipal User { get; }

    public GetAuthStatusQuery(ClaimsPrincipal user)
    {
        User = user;
    }
}
