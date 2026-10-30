using System.Security.Claims;
using FEA.URVP.Application.Features.Auth.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Auth.Queries.GetAuthStatus;

public sealed class GetAuthStatusQuery : IRequest<AuthStatusResponseDto>
{
    public ClaimsPrincipal User { get; }

    public GetAuthStatusQuery(ClaimsPrincipal user)
    {
        User = user;
    }
}
