using FEA.URVP.Domain.Entities.Users;

namespace FEA.URVP.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(User user);
}
