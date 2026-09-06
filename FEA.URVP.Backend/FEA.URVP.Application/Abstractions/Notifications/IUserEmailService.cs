namespace FEA.URVP.Application.Abstractions.Notifications;

public interface IUserEmailService
{
    Task<string?> GetUserEmailAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<string?> GetUserNameAsync(Guid userId, CancellationToken cancellationToken = default);
}
