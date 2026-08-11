using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.DTOs.Users;
using FEA.URVP.Application.Mappings;
using MediatR;

namespace FEA.URVP.Application.Queries.Users.List;

public sealed class ListUsersQueryHandler
    : IRequestHandler<ListUsersQuery, (IReadOnlyList<UserDto> Items, int TotalCount)>
{
    private readonly IUserRepository _users;

    public ListUsersQueryHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<(IReadOnlyList<UserDto> Items, int TotalCount)> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _users.ListAsync(
            request.Search,
            request.Role,
            request.SortBy,
            request.SortDir,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return (items.Select(u => u.ToDto()).ToList(), totalCount);
    }
}
