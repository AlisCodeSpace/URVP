using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces;
using RICHConnect.Backend.Domain.Entities.Users;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Guid>> GetAdminUserIdsAsync()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .Select(u => u.Id)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> HasRoleAsync(Guid userId, UserRole role)
        {
            return await _context.Users
                .AnyAsync(u => u.Id == userId && u.Role == role);
        }

        public async Task<UserRole?> GetUserRoleAsync(Guid userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.Role })
                .FirstOrDefaultAsync();

            return user?.Role;
        }
    }
}
