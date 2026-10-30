using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.ProjectRankings;

internal static class ProjectRankingAccess
{
    public static void EnsureCanRank(UserRole role)
    {
        if (role is UserRole.Student or UserRole.Admin)
        {
            return;
        }

        throw new UnauthorizedAccessException("Only students can rank projects.");
    }

    public const string ProfileRequiredMessage =
        "Complete your student profile before ranking projects.";

    public static void EnsureHasSavedProfile(UserRole role, bool profileExists)
    {
        if (role is UserRole.Admin)
        {
            return;
        }

        if (!profileExists)
        {
            throw new InvalidOperationException(ProfileRequiredMessage);
        }
    }
}
