using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<User> Items, int TotalCount)> ListAsync(
        string? search,
        UserRole? role,
        UserSortField sortBy,
        SortDirection sortDir,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// All users matching the list filters, without paging.
    /// When <paramref name="completedStudentProfilesOnly"/> is true, student
    /// accounts are included only if they have saved a student profile.
    /// </summary>
    Task<IReadOnlyList<User>> ListAllAsync(
        string? search,
        UserRole? role,
        UserSortField sortBy,
        SortDirection sortDir,
        bool completedStudentProfilesOnly,
        CancellationToken cancellationToken = default);

    Task<int> CountByRoleAsync(UserRole role, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> ListUserIdsByRolesAsync(
        IReadOnlyCollection<UserRole> roles,
        CancellationToken cancellationToken = default);

    void Add(User user);
}
