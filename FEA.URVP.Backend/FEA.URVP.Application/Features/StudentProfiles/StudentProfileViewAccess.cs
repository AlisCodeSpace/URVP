using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Features.StudentProfiles;

/// <summary>
/// Who may open another user's student profile and its files.
/// </summary>
public sealed class StudentProfileViewAccess
{
    private readonly IProjectRankingRepository _rankings;
    private readonly IMatchingRunRepository _placements;
    private readonly IProjectAlumniRepository _alumni;
    private readonly IProjectResearchAssistantRepository _assistants;

    public StudentProfileViewAccess(
        IProjectRankingRepository rankings,
        IMatchingRunRepository placements,
        IProjectAlumniRepository alumni,
        IProjectResearchAssistantRepository assistants)
    {
        _rankings = rankings;
        _placements = placements;
        _alumni = alumni;
        _assistants = assistants;
    }

    /// <summary>
    /// Admins always. Faculty when the student ranked one of their projects, is assigned
    /// to one in the current cycle, or was confirmed on one in an archived cycle.
    /// Anyone listed as a research assistant on a project the student is linked to in the same way.
    /// </summary>
    public async Task<bool> CanViewAsync(
        User viewer,
        Guid studentUserId,
        CancellationToken cancellationToken)
    {
        if (viewer.Role is UserRole.Admin)
        {
            return true;
        }

        if (viewer.Role is UserRole.Faculty
            && await FacultyIsLinkedAsync(viewer.Id, studentUserId, cancellationToken))
        {
            return true;
        }

        return await _assistants.CanViewStudentAsync(viewer.Email, studentUserId, cancellationToken);
    }

    private async Task<bool> FacultyIsLinkedAsync(
        Guid facultyUserId,
        Guid studentUserId,
        CancellationToken cancellationToken) =>
        await _rankings.StudentHasRankedFacultyProjectAsync(studentUserId, facultyUserId, cancellationToken)
        || await _placements.StudentAssignedToFacultyProjectAsync(studentUserId, facultyUserId, cancellationToken)
        || await _alumni.StudentWasConfirmedOnFacultyProjectAsync(studentUserId, facultyUserId, cancellationToken);
}
