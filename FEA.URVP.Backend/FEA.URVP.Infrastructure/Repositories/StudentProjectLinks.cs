using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

/// <summary>
/// Whether a student is linked to one of a set of projects: they ranked it,
/// hold a confirmed seat on it in the current cycle, or were confirmed on it
/// in a cycle that has been archived.
/// </summary>
internal static class StudentProjectLinks
{
    public static IQueryable<Guid> PostedBy(AppDbContext db, Guid facultyUserId) =>
        db.Projects.AsNoTracking()
            .Where(project => project.CreatedByUserId == facultyUserId)
            .Select(project => project.Id);

    public static async Task<bool> AnyAsync(
        AppDbContext db,
        Guid studentUserId,
        IQueryable<Guid> projectIds,
        CancellationToken cancellationToken) =>
        await HasRankedAsync(db, studentUserId, projectIds, cancellationToken)
        || await IsConfirmedOnAsync(db, studentUserId, projectIds, cancellationToken)
        || await WasConfirmedOnAsync(db, studentUserId, projectIds, cancellationToken);

    public static Task<bool> HasRankedAsync(
        AppDbContext db,
        Guid studentUserId,
        IQueryable<Guid> projectIds,
        CancellationToken cancellationToken) =>
        db.ProjectRankings.AsNoTracking().AnyAsync(
            ranking => ranking.StudentUserId == studentUserId
                       && projectIds.Contains(ranking.ProjectId),
            cancellationToken);

    public static Task<bool> IsConfirmedOnAsync(
        AppDbContext db,
        Guid studentUserId,
        IQueryable<Guid> projectIds,
        CancellationToken cancellationToken) =>
        db.Placements.AsNoTracking().AnyAsync(
            placement => placement.StudentUserId == studentUserId
                         && placement.Status == PlacementStatus.Confirmed
                         && placement.MatchingRun.SemesterId == placement.Project.SemesterId
                         && projectIds.Contains(placement.ProjectId),
            cancellationToken);

    public static Task<bool> WasConfirmedOnAsync(
        AppDbContext db,
        Guid studentUserId,
        IQueryable<Guid> projectIds,
        CancellationToken cancellationToken) =>
        db.ProjectAlumni.AsNoTracking().AnyAsync(
            alumnus => alumnus.StudentUserId == studentUserId
                       && alumnus.WasConfirmed
                       && projectIds.Contains(alumnus.ProjectId),
            cancellationToken);
}
