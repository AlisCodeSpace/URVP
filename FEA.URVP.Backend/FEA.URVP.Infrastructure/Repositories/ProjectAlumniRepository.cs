using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class ProjectAlumniRepository : IProjectAlumniRepository
{
    private readonly AppDbContext _db;

    public ProjectAlumniRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<bool> AnyForCycleAsync(
        Guid projectId,
        Guid semesterId,
        CancellationToken cancellationToken = default) =>
        _db.ProjectAlumni.AnyAsync(
            a => a.ProjectId == projectId && a.SemesterId == semesterId,
            cancellationToken);

    public async Task<IReadOnlyList<ProjectAlumni>> ListByProjectAsync(
        Guid projectId,
        CancellationToken cancellationToken = default) =>
        await _db.ProjectAlumni
            .AsNoTracking()
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.ArchivedAt)
            .ThenBy(a => a.StudentName)
            .ToListAsync(cancellationToken);

    public void AddRange(IEnumerable<ProjectAlumni> alumni) =>
        _db.ProjectAlumni.AddRange(alumni);
}
