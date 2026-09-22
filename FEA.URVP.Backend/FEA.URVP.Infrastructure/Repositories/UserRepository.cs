using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FEA.URVP.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> ListAsync(
        string? search,
        UserRole? role,
        UserSortField sortBy,
        SortDirection sortDir,
        int pageNumber,
        int pageSize,
        bool facultyWithProjectsOnly,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_db.Users.AsNoTracking(), search, role, facultyWithProjectsOnly);
        var totalCount = await query.CountAsync(cancellationToken);
        query = ApplySort(query, sortBy, sortDir);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<User>> ListAllAsync(
        string? search,
        UserRole? role,
        UserSortField sortBy,
        SortDirection sortDir,
        bool completedStudentProfilesOnly,
        bool facultyWithProjectsOnly,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_db.Users.AsNoTracking(), search, role, facultyWithProjectsOnly);
        if (completedStudentProfilesOnly && role is null or UserRole.Student)
        {
            query = query.Where(u =>
                u.Role != UserRole.Student ||
                _db.StudentProfiles.Any(p => p.UserId == u.Id));
        }

        return await ApplySort(query, sortBy, sortDir).ToListAsync(cancellationToken);
    }

    public Task<int> CountByRoleAsync(UserRole role, CancellationToken cancellationToken = default) =>
        _db.Users.CountAsync(u => u.Role == role, cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListUserIdsByRolesAsync(
        IReadOnlyCollection<UserRole> roles,
        CancellationToken cancellationToken = default)
    {
        if (roles.Count == 0)
        {
            return [];
        }

        return await _db.Users.AsNoTracking()
            .Where(u => roles.Contains(u.Role))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public void Add(User user) => _db.Users.Add(user);

    private IQueryable<User> ApplyFilters(
        IQueryable<User> query,
        string? search,
        UserRole? role,
        bool facultyWithProjectsOnly)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                u.Name.Contains(term) ||
                u.Email.Contains(term) ||
                u.UserName.Contains(term));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (facultyWithProjectsOnly)
        {
            query = query.Where(u =>
                u.Role == UserRole.Faculty &&
                _db.Projects.Any(p => p.CreatedByUserId == u.Id));
        }

        return query;
    }

    private static IQueryable<User> ApplySort(
        IQueryable<User> query,
        UserSortField sortBy,
        SortDirection sortDir)
    {
        var desc = sortDir == SortDirection.Desc;

        return sortBy switch
        {
            UserSortField.Email => desc
                ? query.OrderByDescending(u => u.Email).ThenBy(u => u.Name)
                : query.OrderBy(u => u.Email).ThenBy(u => u.Name),
            UserSortField.Role => desc
                ? query.OrderByDescending(u => u.Role).ThenBy(u => u.Name)
                : query.OrderBy(u => u.Role).ThenBy(u => u.Name),
            _ => desc
                ? query.OrderByDescending(u => u.Name).ThenBy(u => u.Email)
                : query.OrderBy(u => u.Name).ThenBy(u => u.Email),
        };
    }
}
