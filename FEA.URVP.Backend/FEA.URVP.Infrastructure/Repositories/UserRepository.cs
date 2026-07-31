using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Domain.Entities.Users;
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
        _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public void Add(User user) => _db.Users.Add(user);
}
