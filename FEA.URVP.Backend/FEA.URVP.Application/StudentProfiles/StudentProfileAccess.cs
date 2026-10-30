using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.StudentProfiles;

internal static class StudentProfileAccess
{
    public static void EnsureCanManage(UserRole role)
    {
        if (role is UserRole.Student or UserRole.Admin)
        {
            return;
        }

        throw new UnauthorizedAccessException("Only students can manage a student profile.");
    }
}
