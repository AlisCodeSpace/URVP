using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.StudentProfiles;

internal static class StudentProfileAccess
{
    /// <summary>Temporary FE testing override — remove when Student role assignment exists.</summary>
    private static readonly HashSet<string> StudentRoleOverrides =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "ali.anani@aub.edu.lb",
        };

    public static void EnsureCanManage(UserRole role, string email)
    {
        if (role is UserRole.Student or UserRole.Admin)
        {
            return;
        }

        if (StudentRoleOverrides.Contains(email))
        {
            return;
        }

        throw new UnauthorizedAccessException("Only students can manage a student profile.");
    }

    /// <summary>
    /// Faculty may open a student profile when that student ranked one of their
    /// projects, is assigned to one in the current cycle, or was assigned in an
    /// earlier cycle that has been archived.
    /// </summary>
    public static async Task<bool> FacultyMayViewStudentAsync(
        Guid facultyUserId,
        Guid studentUserId,
        IProjectRankingRepository rankings,
        IMatchingRunRepository placements,
        IProjectAlumniRepository alumni,
        CancellationToken cancellationToken)
    {
        if (await rankings.StudentHasRankedFacultyProjectAsync(
                studentUserId,
                facultyUserId,
                cancellationToken))
        {
            return true;
        }

        if (await placements.StudentAssignedToFacultyProjectAsync(
                studentUserId,
                facultyUserId,
                cancellationToken))
        {
            return true;
        }

        return await alumni.StudentWasConfirmedOnFacultyProjectAsync(
            studentUserId,
            facultyUserId,
            cancellationToken);
    }

    public static async Task EnsureFacultyMayViewStudentAsync(
        Guid facultyUserId,
        Guid studentUserId,
        IProjectRankingRepository rankings,
        IMatchingRunRepository placements,
        IProjectAlumniRepository alumni,
        CancellationToken cancellationToken)
    {
        if (await FacultyMayViewStudentAsync(
                facultyUserId,
                studentUserId,
                rankings,
                placements,
                alumni,
                cancellationToken))
        {
            return;
        }

        throw new UnauthorizedAccessException(
            "You can only view profiles of students who ranked or were assigned to a project you posted.");
    }
}
