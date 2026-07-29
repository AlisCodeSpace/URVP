using RICHConnect.Backend.Domain.Entities.Users;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Infrastructure.Data.Repositories.Users.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>
        /// Get all admin user IDs
        /// </summary>
        Task<List<Guid>> GetAdminUserIdsAsync();
        
        /// <summary>
        /// Get user by ID
        /// </summary>
        Task<User?> GetByIdAsync(Guid userId);
        
        /// <summary>
        /// Check if user has a specific role
        /// </summary>
        Task<bool> HasRoleAsync(Guid userId, UserRole role);
        
        /// <summary>
        /// Get user role
        /// </summary>
        Task<UserRole?> GetUserRoleAsync(Guid userId);
    }
}
