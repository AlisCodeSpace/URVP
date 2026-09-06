using FEA.URVP.Application.Abstractions.Notifications;
using FEA.URVP.Application.Abstractions.Persistence;

namespace FEA.URVP.Application.Services.Notifications;

public sealed class UserEmailService : IUserEmailService
{
    private readonly IUserRepository _users;

    public UserEmailService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<string?> GetUserEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId, cancellationToken);
        return string.IsNullOrWhiteSpace(user?.Email) ? null : user.Email;
    }

    public async Task<string?> GetUserNameAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(userId, cancellationToken);
        return string.IsNullOrWhiteSpace(user?.Name) ? null : user.Name;
    }
}
