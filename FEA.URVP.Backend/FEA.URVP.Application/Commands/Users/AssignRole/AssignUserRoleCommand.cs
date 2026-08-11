using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Commands.Users.AssignRole;

public sealed class AssignUserRoleCommand : IRequest<UserDto>
{
    [JsonIgnore]
    public Guid UserId { get; set; }

    [JsonIgnore]
    public Guid CurrentUserId { get; set; }

    public UserRole Role { get; init; }
}
