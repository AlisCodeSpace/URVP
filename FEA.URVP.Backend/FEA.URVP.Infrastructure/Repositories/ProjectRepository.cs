using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly AppDbContext _db;

    public ProjectRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Project?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> ListAsync(
        Guid? createdByUserId,
        ProjectStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Projects.AsNoTracking().AsQueryable();

        if (createdByUserId.HasValue)
        {
            query = query.Where(p => p.CreatedByUserId == createdByUserId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> ListForAdminAsync(
        string? search,
        ProjectStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Projects.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                p.Title.Contains(term) ||
                p.FacultyNameSnapshot.Contains(term) ||
                p.AffiliationSnapshot.Contains(term) ||
                p.EmailSnapshot.Contains(term));
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Project project) => _db.Projects.Add(project);

    public void Remove(Project project) => _db.Projects.Remove(project);
}
