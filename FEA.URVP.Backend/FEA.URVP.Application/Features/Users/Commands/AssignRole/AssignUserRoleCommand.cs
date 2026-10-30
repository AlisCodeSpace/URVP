using System.Text.Json.Serialization;
using FEA.URVP.Application.Features.Users.DTOs;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Features.Users.Commands.AssignRole;

public sealed class AssignUserRoleCommand : IRequest<UserDto>
{
    [JsonIgnore]
    public Guid UserId { get; set; }

    [JsonIgnore]
    public Guid CurrentUserId { get; set; }

    public UserRole Role { get; init; }
}
