using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.DTOs.Users;

public sealed class UserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string UserName { get; init; } = null!;
    public string Affiliation { get; init; } = null!;
    public UserRole Role { get; init; }
    public string RoleLabel { get; init; } = null!;
    public DateTime RegisteredAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
