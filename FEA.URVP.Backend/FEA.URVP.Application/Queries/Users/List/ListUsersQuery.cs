using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Domain.Enums;
using MediatR;

namespace FEA.URVP.Application.Queries.Users.List;

public sealed class ListUsersQuery : IRequest<(IReadOnlyList<UserDto> Items, int TotalCount)>
{
    public string? Search { get; }
    public UserRole? Role { get; }
    public UserSortField SortBy { get; }
    public SortDirection SortDir { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public ListUsersQuery(
        string? search,
        UserRole? role,
        UserSortField sortBy,
        SortDirection sortDir,
        int pageNumber,
        int pageSize)
    {
        Search = search;
        Role = role;
        SortBy = sortBy;
        SortDir = sortDir;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
