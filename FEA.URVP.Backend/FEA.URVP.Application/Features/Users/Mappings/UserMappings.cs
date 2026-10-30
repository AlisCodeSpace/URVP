using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Mappings;

public static class UserMappings
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        UserName = user.UserName,
        Affiliation = user.Affiliation,
        Role = user.Role,
        RoleLabel = ToLabel(user.Role),
        RegisteredAt = user.RegisteredAt,
        UpdatedAt = user.UpdatedAt
    };

    public static string ToLabel(UserRole role) => role switch
    {
        UserRole.Student => "Student",
        UserRole.Faculty => "Faculty",
        UserRole.Admin => "Admin",
        _ => role.ToString()
    };
}
